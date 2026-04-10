OrbitRenderer.init({
    canvasId: 'heroOrbitCanvas',
    satellites: [
        {
            id: 'demo-iss',
            name: 'ISS-TYPE',
            orbit: {
                semiMajorAxis:     6778000,
                eccentricity:      0.0001,
                inclination:       51.6,
                raan:              0,
                argumentOfPerigee: 0,
                meanAnomaly:       0
            }
        },
        {
            id: 'demo-sso',
            name: 'SUN-SYNC',
            orbit: {
                semiMajorAxis:     6928000,
                eccentricity:      0.001,
                inclination:       98.1,
                raan:              60,
                argumentOfPerigee: 90,
                meanAnomaly:       120
            }
        },
        {
            id: 'demo-polar',
            name: 'POLAR',
            orbit: {
                semiMajorAxis:     7071000,
                eccentricity:      0.002,
                inclination:       90.0,
                raan:              150,
                argumentOfPerigee: 45,
                meanAnomaly:       240
            }
        }
    ],
    timeScale:  400,
    showLabels: false,
});