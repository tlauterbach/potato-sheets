# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.3] - 2023-09-09
### Fixed
- issue where ContentAttribute fields/properties without an alias did not default to the field/property name

## [1.0.2] - 2023-09-08
### Fixed
- issue where Rows without PrimaryValues would throw an error. Behaviour now is to  skip them if the PrimaryValue is null or empty

## [1.0.1] - 2023-09-08
### Added
- missing .meta files for documentation to avoid errors

## [1.0.0] - 2023-09-08
### Added
- initial importer that polls data from GoogleSheets
