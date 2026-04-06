/**
 * orbitalMechanics.js
 * Pure Keplerian orbital mechanics — no DOM, no rendering.
 * All angles in RADIANS internally. Input from C# model is in degrees.
 */

const OrbitalMechanics = (() => {

    const TWO_PI = 2 * Math.PI;

    // ─── Unit helpers ──────────────────────────────────────────────────────────

    function deg2rad(d) { return d * Math.PI / 180; }
    function rad2deg(r) { return r * 180 / Math.PI; }

    // ─── Kepler's equation solver (Newton-Raphson) ─────────────────────────────
    // Solves: E - e*sin(E) = M   for E (eccentric anomaly)
    // Converges in ~5 iterations for e < 0.99

    function solveKepler(M, e) {
        M = ((M % TWO_PI) + TWO_PI) % TWO_PI; // normalise to [0, 2π]
        let E = e < 0.8 ? M : Math.PI;        // initial guess
        for (let i = 0; i < 100; i++) {
            const dE = (M - E + e * Math.sin(E)) / (1 - e * Math.cos(E));
            E += dE;
            if (Math.abs(dE) < 1e-10) break;
        }
        return E;
    }

    // ─── True anomaly from eccentric anomaly ──────────────────────────────────

    function eccentricToTrue(E, e) {
        return 2 * Math.atan2(
            Math.sqrt(1 + e) * Math.sin(E / 2),
            Math.sqrt(1 - e) * Math.cos(E / 2)
        );
    }

    // ─── Orbital radius at a given true anomaly ───────────────────────────────

    function orbitalRadius(a, e, nu) {
        return a * (1 - e * e) / (1 + e * Math.cos(nu));
    }

    // ─── Perifocal → ECI (Earth-Centered Inertial) rotation matrix ───────────
    // Applies three Euler rotations: ω (argPerigee), i (inclination), Ω (RAAN)

    function perifocalToECI(omega, inc, raan) {
        const cosO = Math.cos(raan),  sinO = Math.sin(raan);
        const cosI = Math.cos(inc),   sinI = Math.sin(inc);
        const cosW = Math.cos(omega), sinW = Math.sin(omega);

        // 3×3 rotation matrix (row-major)
        return [
            [ cosO*cosW - sinO*sinW*cosI,  -cosO*sinW - sinO*cosW*cosI,  sinO*sinI ],
            [ sinO*cosW + cosO*sinW*cosI,  -sinO*sinW + cosO*cosW*cosI, -cosO*sinI ],
            [ sinW*sinI,                    cosW*sinI,                    cosI      ]
        ];
    }

    // ─── Apply rotation matrix to [x, y, z] ──────────────────────────────────

    function applyRotation(R, x, y, z) {
        return {
            x: R[0][0]*x + R[0][1]*y + R[0][2]*z,
            y: R[1][0]*x + R[1][1]*y + R[1][2]*z,
            z: R[2][0]*x + R[2][1]*y + R[2][2]*z
        };
    }

    // ─── ECI 3D → isometric 2D screen projection ─────────────────────────────
    // A simple axonometric projection that gives a nice "tilted globe" feel.
    // camTilt: how much the view tips forward (radians); 0 = top-down, π/4 = angled.

    function projectECI(pos3d, scale, camTilt) {
        const tilt = camTilt ?? 0.5;  // ~28°
        const cosT = Math.cos(tilt);
        const sinT = Math.sin(tilt);
        // Rotate around X axis (tilt the north pole toward viewer)
        const screenY3d = -pos3d.y * cosT + pos3d.z * sinT;
        return {
            sx: pos3d.x * scale,
            sy: screenY3d * scale
        };
    }

    // ─── Public: satellite position at time t ─────────────────────────────────
    /**
     * @param {object} params  - OrbitParameters from C# (degrees, metres)
     * @param {number} t       - elapsed seconds since epoch
     * @param {number} scale   - metres-to-pixels scale factor
     * @param {number} [camTilt] - camera tilt in radians (default 0.5)
     * @returns {{ sx, sy, r3d }} screen position + 3D radius (metres)
     */
    function satellitePosition(params, t, scale, camTilt) {
        const a     = params.semiMajorAxis;
        const e     = params.eccentricity;
        const inc   = deg2rad(params.inclination);
        const raan  = deg2rad(params.raan);
        const omega = deg2rad(params.argumentOfPerigee);
        const M0    = deg2rad(params.meanAnomaly);

        // Mean motion (rad/s)
        const GM = 3.986004418e14; // μ = GM for Earth, m³/s²
        const n  = Math.sqrt(GM / (a * a * a));

        // Mean anomaly at time t
        const M = M0 + n * t;

        // Solve for eccentric anomaly
        const E  = solveKepler(M, e);
        const nu = eccentricToTrue(E, e);
        const r  = orbitalRadius(a, e, nu);

        // Position in perifocal frame
        const xPeri = r * Math.cos(nu);
        const yPeri = r * Math.sin(nu);

        // Rotate to ECI
        const R   = perifocalToECI(omega, inc, raan);
        const eci = applyRotation(R, xPeri, yPeri, 0);

        // Project to screen
        const screen = projectECI(eci, scale, camTilt);

        return { sx: screen.sx, sy: screen.sy, r3d: r };
    }

    // ─── Public: full orbit path as array of screen points ────────────────────
    /**
     * @param {object} params  - OrbitParameters
     * @param {number} scale
     * @param {number} [steps=120]  - number of points (more = smoother)
     * @param {number} [camTilt]
     * @returns {Array<{sx, sy}>}
     */
    function getOrbitPath(params, scale, steps, camTilt) {
        steps = steps ?? 120;
        const a     = params.semiMajorAxis;
        const e     = params.eccentricity;
        const inc   = deg2rad(params.inclination);
        const raan  = deg2rad(params.raan);
        const omega = deg2rad(params.argumentOfPerigee);
        const R     = perifocalToECI(omega, inc, raan);

        const points = [];
        for (let i = 0; i <= steps; i++) {
            const nu  = (TWO_PI * i) / steps;
            const r   = orbitalRadius(a, e, nu);
            const xP  = r * Math.cos(nu);
            const yP  = r * Math.sin(nu);
            const eci = applyRotation(R, xP, yP, 0);
            const s   = projectECI(eci, scale, camTilt);
            points.push(s);
        }
        return points;
    }

    // ─── Public: orbital period in seconds ────────────────────────────────────

    function orbitalPeriod(semiMajorAxis) {
        const GM = 3.986004418e14;
        return TWO_PI * Math.sqrt((semiMajorAxis ** 3) / GM);
    }

    // ─── Public: compute a sensible scale so orbits fit in a given pixel box ──
    /**
     * @param {Array<object>} allParams  - array of OrbitParameters
     * @param {number} boxRadius         - pixels from centre to edge of canvas area
     * @returns {number} scale (pixels per metre)
     */
    function autoScale(allParams, boxRadius) {
        let maxRadius = 0;
        for (const p of allParams) {
            const apoapsis = p.semiMajorAxis * (1 + p.eccentricity);
            if (apoapsis > maxRadius) maxRadius = apoapsis;
        }
        return (boxRadius * 0.85) / maxRadius;
    }

    return {
        satellitePosition,
        getOrbitPath,
        orbitalPeriod,
        autoScale,
        deg2rad,
        rad2deg
    };

})();

// CommonJS + browser global dual export
if (typeof module !== 'undefined' && module.exports) {
    module.exports = OrbitalMechanics;
}