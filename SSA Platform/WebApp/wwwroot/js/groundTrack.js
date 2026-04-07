/**
 * groundTrack.js
 * Extends the orbital mechanics with ground track computation.
 * Requires orbitalMechanics.js to be loaded first.
 *
 * Usage:
 *   const track = GroundTrack.compute(orbitParams, { orbits: 3 });
 *   // returns array of { lat, lng, t } points
 *
 *   const pos = GroundTrack.currentPosition(orbitParams);
 *   // returns { lat, lng, alt, velocity }
 */

const GroundTrack = (() => {

    const TWO_PI          = 2 * Math.PI;
    const GM              = 3.986004418e14;   // m³/s²
    const EARTH_RADIUS    = 6_371_000;        // m
    const EARTH_ROT_RATE  = 7.2921150e-5;     // rad/s  (sidereal rotation)
    const DEG             = 180 / Math.PI;
    const RAD             = Math.PI / 180;

    // ── Kepler solver (duplicated here so groundTrack.js is self-contained) ──

    function solveKepler(M, e) {
        M = ((M % TWO_PI) + TWO_PI) % TWO_PI;
        let E = e < 0.8 ? M : Math.PI;
        for (let i = 0; i < 100; i++) {
            const dE = (M - E + e * Math.sin(E)) / (1 - e * Math.cos(E));
            E += dE;
            if (Math.abs(dE) < 1e-10) break;
        }
        return E;
    }

    function trueAnomaly(E, e) {
        return 2 * Math.atan2(
            Math.sqrt(1 + e) * Math.sin(E / 2),
            Math.sqrt(1 - e) * Math.cos(E / 2)
        );
    }

    // ── ECI position at time t ─────────────────────────────────────────────

    function eciPosition(p, t) {
        const a     = p.semiMajorAxis;
        const e     = p.eccentricity;
        const inc   = p.inclination       * RAD;
        const raan  = p.raan              * RAD;
        const omega = p.argumentOfPerigee * RAD;
        const M0    = p.meanAnomaly       * RAD;

        const n  = Math.sqrt(GM / (a * a * a));
        const M  = M0 + n * t;
        const E  = solveKepler(M, e);
        const nu = trueAnomaly(E, e);
        const r  = a * (1 - e * e) / (1 + e * Math.cos(nu));

        // Perifocal coordinates
        const xP = r * Math.cos(nu);
        const yP = r * Math.sin(nu);

        // Rotation matrix components
        const cosO = Math.cos(raan),  sinO = Math.sin(raan);
        const cosI = Math.cos(inc),   sinI = Math.sin(inc);
        const cosW = Math.cos(omega), sinW = Math.sin(omega);

        return {
            x: (cosO*cosW - sinO*sinW*cosI)*xP + (-cosO*sinW - sinO*cosW*cosI)*yP,
            y: (sinO*cosW + cosO*sinW*cosI)*xP + (-sinO*sinW + cosO*cosW*cosI)*yP,
            z: (sinW*sinI)*xP                  + (cosW*sinI)*yP,
            r
        };
    }

    // ── ECI → geodetic lat/lng ─────────────────────────────────────────────
    // Uses Greenwich Sidereal Time to account for Earth's rotation.
    // t0 is the epoch offset in seconds from a reference (we use 0 for simplicity
    // since absolute GST doesn't matter for relative ground track shape).

    function eciToLatLng(eci, t) {
        // Earth rotation since epoch
        const theta = EARTH_ROT_RATE * t;

        // Rotate ECI → ECEF
        const cosT = Math.cos(theta), sinT = Math.sin(theta);
        const xECEF =  eci.x * cosT + eci.y * sinT;
        const yECEF = -eci.x * sinT + eci.y * cosT;
        const zECEF =  eci.z;

        // ECEF → lat/lng (spherical Earth — good enough for visualisation)
        const rXY  = Math.sqrt(xECEF * xECEF + yECEF * yECEF);
        const lat  = Math.atan2(zECEF, rXY) * DEG;
        let   lng  = Math.atan2(yECEF, xECEF) * DEG;

        // Normalise longitude to [-180, 180]
        lng = ((lng + 540) % 360) - 180;

        return { lat, lng };
    }

    // ── Orbital period ─────────────────────────────────────────────────────

    function period(p) {
        return TWO_PI * Math.sqrt((p.semiMajorAxis ** 3) / GM);
    }

    // ── Public: compute ground track ───────────────────────────────────────
    /**
     * @param {object} params        - orbit parameters (degrees + metres)
     * @param {object} [options]
     * @param {number} [options.orbits=3]      - how many full orbits to compute
     * @param {number} [options.stepsPerOrbit=180] - resolution
     * @param {number} [options.tOffset=0]     - start time offset in seconds
     * @returns {Array<{lat, lng, t}>}
     */
    function compute(params, options = {}) {
        const orbits        = options.orbits        ?? 3;
        const stepsPerOrbit = options.stepsPerOrbit ?? 180;
        const tOffset       = options.tOffset       ?? 0;

        const T      = period(params);
        const total  = T * orbits;
        const steps  = stepsPerOrbit * orbits;
        const dt     = total / steps;

        const points = [];
        let prevLng  = null;

        for (let i = 0; i <= steps; i++) {
            const t   = tOffset + i * dt;
            const eci = eciPosition(params, t);
            const ll  = eciToLatLng(eci, t);

            // Detect antimeridian crossing — insert a break marker
            // so Leaflet doesn't draw a line across the whole map
            if (prevLng !== null && Math.abs(ll.lng - prevLng) > 180) {
                points.push(null); // break marker
            }

            points.push({ lat: ll.lat, lng: ll.lng, t });
            prevLng = ll.lng;
        }

        return points;
    }

    // ── Public: current sub-satellite position ─────────────────────────────
    /**
     * @param {object} params
     * @param {number} [tOffset=0]
     * @returns {{ lat, lng, altitudeKm, velocityKms }}
     */
    function currentPosition(params, tOffset = 0) {
        const eci = eciPosition(params, tOffset);
        const ll  = eciToLatLng(eci, tOffset);
        const altitudeKm  = (eci.r - EARTH_RADIUS) / 1000;

        // Orbital velocity (vis-viva)
        const v = Math.sqrt(GM * (2 / eci.r - 1 / params.semiMajorAxis));
        const velocityKms = v / 1000;

        return { lat: ll.lat, lng: ll.lng, altitudeKm, velocityKms };
    }

    // ── Public: coverage footprint radius in degrees ───────────────────────
    // Approximate ground coverage circle for a given altitude.
    // minElevation: minimum elevation angle in degrees (default 5°)

    function footprintRadiusDeg(altitudeKm, minElevationDeg = 5) {
        const re   = EARTH_RADIUS / 1000; // km
        const h    = altitudeKm;
        const elev = minElevationDeg * RAD;
        // Central angle from subsatellite point to horizon
        const rho  = Math.acos(re / (re + h) * Math.cos(elev)) - elev;
        return rho * DEG;
    }

    return { compute, currentPosition, footprintRadiusDeg, period };

})();
