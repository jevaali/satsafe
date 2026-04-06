/**
 * orbitRenderer.js
 * Reusable Keplerian orbit renderer for canvas elements.
 * No globals, no auto-init — call OrbitRenderer.init(config) explicitly.
 *
 * Usage:
 *   OrbitRenderer.init({
 *     canvasId:   'orbitCanvas',   // required — ID of the <canvas> element
 *     satellites: [...],           // required — array of satellite objects
 *     timeScale:  500,             // optional — sim seconds per wall second (default 500)
 *     showLabels: true,            // optional — draw name labels (default true)
 *     trailLength: 55,             // optional — frames of trail (default 55)
 *     camTilt:    0.52,            // optional — camera tilt in radians (default 0.52)
 *     earthColor: '#4a90d9',       // optional — Earth mid colour (default blue)
 *     onReady:    () => {},        // optional — called once after first frame
 *   });
 *
 * Each satellite object:
 *   {
 *     id:   string,
 *     name: string,
 *     orbit: {
 *       semiMajorAxis:     number,  // metres
 *       eccentricity:      number,
 *       inclination:       number,  // degrees
 *       raan:              number,  // degrees
 *       argumentOfPerigee: number,  // degrees
 *       meanAnomaly:       number   // degrees
 *     }
 *   }
 *
 * Returned instance API:
 *   instance.stop()               — pause animation
 *   instance.start()              — resume animation
 *   instance.update(satellites)   — hot-swap satellite data
 *   instance.destroy()            — remove all listeners and stop
 */

const OrbitRenderer = (() => {

    // ── Default config ────────────────────────────────────────────────────────

    const DEFAULTS = {
        timeScale:   500,
        showLabels:  true,
        trailLength: 55,
        camTilt:     0.52,
        earthColor:  '#4a90d9',
        orbitSteps:  180,
        onReady:     null,
    };

    // ── Satellite colours — index-based so each satellite gets its own colour ─
    // Falls back to cycling if there are more satellites than colours.

    const SAT_PALETTE = [
        { main: '#4fc3f7', trail: 'rgba(79,195,247,',   glow: 'rgba(79,195,247,'   }, // light blue
        { main: '#81c784', trail: 'rgba(129,199,132,',  glow: 'rgba(129,199,132,'  }, // green
        { main: '#ffb74d', trail: 'rgba(255,183,77,',   glow: 'rgba(255,183,77,'   }, // amber
        { main: '#f06292', trail: 'rgba(240,98,146,',   glow: 'rgba(240,98,146,'   }, // pink
        { main: '#ce93d8', trail: 'rgba(206,147,216,',  glow: 'rgba(206,147,216,'  }, // purple
        { main: '#80cbc4', trail: 'rgba(128,203,196,',  glow: 'rgba(128,203,196,'  }, // teal
    ];

    // Deterministic star field (no Math.random so it never shifts between frames)
    const STARS = Array.from({ length: 90 }, (_, i) => ({
        x: Math.sin(i * 127.1) * 0.5 + 0.5,
        y: Math.sin(i * 311.7) * 0.5 + 0.5,
        r: 0.4 + (Math.sin(i * 74.3) * 0.5 + 0.5) * 1.1,
        a: 0.25 + (Math.sin(i * 53.1) * 0.5 + 0.5) * 0.55,
    }));

    // ── Factory ───────────────────────────────────────────────────────────────

    function init(userConfig) {
        const cfg = { ...DEFAULTS, ...userConfig };

        if (!cfg.canvasId)   throw new Error('OrbitRenderer.init: canvasId is required');
        if (!cfg.satellites) throw new Error('OrbitRenderer.init: satellites is required');

        const canvas = document.getElementById(cfg.canvasId);
        if (!canvas) throw new Error(`OrbitRenderer.init: canvas #${cfg.canvasId} not found`);

        const ctx = canvas.getContext('2d');

        // ── Instance state ────────────────────────────────────────────────────

        let W, H, cx, cy, scale;
        let sats = [];
        let trails = new Map();
        let elapsedSec = 0;
        let lastTs = null;
        let rafId = null;
        let glowPhase = 0;
        let readyFired = false;

        // ── Layout ────────────────────────────────────────────────────────────

        function resize() {
            const rect = canvas.parentElement.getBoundingClientRect();
            W = canvas.width  = rect.width  || 480;
            H = canvas.height = rect.height || 320;
            cx = W / 2;
            cy = H / 2;
        }

        function recomputeScale() {
            if (sats.length === 0) return;
            scale = OrbitalMechanics.autoScale(
                sats.map(s => s.orbit),
                Math.min(W, H) / 2
            );
        }

        function loadSatellites(raw) {
            sats = raw.map((s, i) => ({
                ...s,
                _color: SAT_PALETTE[i % SAT_PALETTE.length],
                period: OrbitalMechanics.orbitalPeriod(s.orbit.semiMajorAxis),
            }));
            trails = new Map(sats.map(s => [s.id, []]));
        }

        // ── Animation loop ────────────────────────────────────────────────────

        function tick(ts) {
            if (lastTs !== null) elapsedSec += ((ts - lastTs) / 1000) * cfg.timeScale;
            lastTs = ts;
            glowPhase += 0.05;
            render();
            if (!readyFired && cfg.onReady) { cfg.onReady(); readyFired = true; }
            rafId = requestAnimationFrame(tick);
        }

        // ── Render ────────────────────────────────────────────────────────────

        function render() {
            ctx.clearRect(0, 0, W, H);
            drawStars();
            drawEarth();
            drawOrbitPaths();
            drawSatellites();
        }

        function drawStars() {
            for (const s of STARS) {
                ctx.beginPath();
                ctx.arc(s.x * W, s.y * H, s.r, 0, Math.PI * 2);
                ctx.fillStyle = `rgba(255,255,255,${s.a})`;
                ctx.fill();
            }
        }

        function drawEarth() {
            const R = Math.min(W, H) * 0.075; // Earth scales with canvas size

            // Atmosphere
            const atm = ctx.createRadialGradient(cx, cy, R * 0.85, cx, cy, R * 1.75);
            atm.addColorStop(0, 'rgba(74,144,217,0.22)');
            atm.addColorStop(1, 'rgba(74,144,217,0)');
            ctx.beginPath(); ctx.arc(cx, cy, R * 1.75, 0, Math.PI * 2);
            ctx.fillStyle = atm; ctx.fill();

            // Body
            const body = ctx.createRadialGradient(cx - R * .3, cy - R * .3, R * .1, cx, cy, R);
            body.addColorStop(0, lighten(cfg.earthColor, 40));
            body.addColorStop(0.5, cfg.earthColor);
            body.addColorStop(1, darken(cfg.earthColor, 40));
            ctx.beginPath(); ctx.arc(cx, cy, R, 0, Math.PI * 2);
            ctx.fillStyle = body; ctx.fill();

            // Highlight
            ctx.beginPath();
            ctx.arc(cx - R * .18, cy - R * .22, R * .28, 0, Math.PI * 2);
            ctx.fillStyle = 'rgba(255,255,255,0.07)'; ctx.fill();

            // Border
            ctx.beginPath(); ctx.arc(cx, cy, R, 0, Math.PI * 2);
            ctx.strokeStyle = 'rgba(255,255,255,0.18)'; ctx.lineWidth = 1; ctx.stroke();
        }

        function drawOrbitPaths() {
            ctx.setLineDash([4, 7]);
            ctx.lineWidth = 1;
            for (const sat of sats) {
                const pts = OrbitalMechanics.getOrbitPath(sat.orbit, scale, cfg.orbitSteps, cfg.camTilt);
                if (pts.length < 2) continue;
                ctx.beginPath();
                ctx.moveTo(cx + pts[0].sx, cy - pts[0].sy);
                for (let i = 1; i < pts.length; i++) ctx.lineTo(cx + pts[i].sx, cy - pts[i].sy);
                ctx.closePath();
                // Use a tinted version of the satellite's own colour for its orbit track
                ctx.strokeStyle = sat._color.trail + '0.25)';
                ctx.stroke();
            }
            ctx.setLineDash([]);
        }

        function drawSatellites() {
            for (const sat of sats) {
                const pos = OrbitalMechanics.satellitePosition(sat.orbit, elapsedSec, scale, cfg.camTilt);
                const sx  = cx + pos.sx;
                const sy  = cy - pos.sy;
                const C   = sat._color;

                // Trail
                const trail = trails.get(sat.id);
                trail.push({ sx, sy });
                if (trail.length > cfg.trailLength) trail.shift();

                for (let i = 1; i < trail.length; i++) {
                    ctx.beginPath();
                    ctx.moveTo(trail[i-1].sx, trail[i-1].sy);
                    ctx.lineTo(trail[i].sx,   trail[i].sy);
                    ctx.strokeStyle = C.trail + (i / trail.length * 0.5) + ')';
                    ctx.lineWidth = 1.5;
                    ctx.stroke();
                }

                // Dot
                ctx.beginPath(); ctx.arc(sx, sy, 5, 0, Math.PI * 2);
                ctx.fillStyle = C.main; ctx.fill();
                ctx.strokeStyle = 'rgba(255,255,255,0.65)'; ctx.lineWidth = 1; ctx.stroke();

                // Label
                if (cfg.showLabels && sat.name && W > 300) {
                    ctx.font = '11px system-ui, sans-serif';
                    ctx.fillStyle = 'rgba(220,230,240,0.82)';
                    ctx.fillText(sat.name, sx + 8, sy - 5);
                }
            }
        }

        function drawEmptyState() {
            ctx.font = '14px system-ui, sans-serif';
            ctx.fillStyle = 'rgba(108,117,125,0.6)';
            ctx.textAlign = 'center';
            ctx.fillText('No satellites loaded', cx, cy);
        }

        // ── Colour helpers ────────────────────────────────────────────────────

        function lighten(hex, amount) { return shiftColor(hex, amount); }
        function darken(hex, amount)  { return shiftColor(hex, -amount); }

        function shiftColor(hex, amount) {
            const n = parseInt(hex.replace('#',''), 16);
            const r = Math.min(255, Math.max(0, (n >> 16) + amount));
            const g = Math.min(255, Math.max(0, ((n >> 8) & 0xff) + amount));
            const b = Math.min(255, Math.max(0, (n & 0xff) + amount));
            return `rgb(${r},${g},${b})`;
        }

        // ── Resize listener ───────────────────────────────────────────────────

        const onResize = () => { resize(); recomputeScale(); };
        window.addEventListener('resize', onResize);

        // ── Bootstrap ─────────────────────────────────────────────────────────

        resize();

        if (cfg.satellites.length === 0) {
            drawEmptyState();
        } else {
            loadSatellites(cfg.satellites);
            recomputeScale();
            rafId = requestAnimationFrame(tick);
        }

        // ── Public instance API ───────────────────────────────────────────────

        return {
            stop() {
                if (rafId) { cancelAnimationFrame(rafId); rafId = null; }
            },
            start() {
                if (!rafId) { lastTs = null; rafId = requestAnimationFrame(tick); }
            },
            update(newSatellites) {
                this.stop();
                elapsedSec = 0;
                lastTs = null;
                readyFired = false;
                loadSatellites(newSatellites);
                recomputeScale();
                rafId = requestAnimationFrame(tick);
            },
            destroy() {
                this.stop();
                window.removeEventListener('resize', onResize);
                ctx.clearRect(0, 0, W, H);
            },
        };
    }

    return { init };

})();