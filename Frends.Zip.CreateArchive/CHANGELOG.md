# Changelog

## [2.0.0] - 2026-11-03

### Changed

- **[Breaking Change]** Merged the `Source` and `Destination` parameter tabs into a single `Input` tab. Renamed `Source.Directory` to `SourceDirectory`, `Destination.Directory` to `DestinationDirectory` and `Destination.FileName` to `DestinationFileName`; other source and destination properties keep their names.
- **[Breaking Change]** The Task result now includes `Success` and `Error` properties.
- Added `ThrowErrorOnFailure` and `ErrorMessageOnFailure` options. By default the Task still throws an error on failure; when `ThrowErrorOnFailure` is disabled, the error is returned in the result instead.
- Upgraded the Task to .NET 8.

## [1.3.0] - 2025-12-29

### Fixed

- Fix repository url in metadata

## [1.2.0] - 2024-12-13

### Changed

- Drop DotNetZip in favour of ProDotNetZip because of security reasons
- DotNetZip has a HIGH severity directory traversal vulnerability (CVE reported Nov 2024) affecting versions 1.10.1
  through 1.16.0 with no patch available (package is deprecated)
- The migration to ProDotNetZip 1.20.0 addresses this security concern

## [1.1.0] - 2023-11-27

### Added

- [Breaking] Added Encoding for file and directory names.

## [1.0.0] - 2022-02-23

### Added

- Initial implementation
