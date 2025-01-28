# Changelog

## [1.2.0] - 2024-12-13
### Changed
- Drop DotNetZip in favour of ProDotNetZip because of security reasons
- DotNetZip has a HIGH severity directory traversal vulnerability (CVE reported Nov 2024) affecting versions 1.10.1 through 1.16.0 with no patch available (package is deprecated)
- The migration to ProDotNetZip 1.20.0 addresses this security concern

## [1.1.0] - 2023-11-27
### Added
- [Breaking] Added Encoding for file and directory names.

## [1.0.0] - 2022-02-23
### Added
- Initial implementation
