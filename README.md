# taxcalc-api

An api to calculate income tax according to predefined tax bands.

- To stand the application up, some critical secrets are needed:
  - loggingUri - Chosen loki endpoint for logging
  - loggingLogin - SA login for loki
  - loggingToken - loki token
  - dbConnectionString - Connection string for the relational database containing the tax bands

- Github actions will build and test the solution at each push to a branch.
- CI/CD deploy still pending.
