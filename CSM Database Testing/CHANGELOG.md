# CSM Database Testing CHANGELOG

## [7.0.2] - 18.06-2026

### Fixed

- Fixed code style

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.1.0            | 4.0.0           |
| xunit.v3								  | 3.2.2            | 3.2.2           |

## [7.0.1] - 18.06-2026

### Fixed

- Fixed some [Store] method errors at [TestingStoreManager] that were missing entity sanitization logic.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.1.0            | 4.0.0           |
| xunit.v3								  | 3.2.2            | 3.2.2           |

## [7.0.0] - 18.06-2026

### Fixed

- Exposing as virtual some missing test cases from [DepotTestsBase].

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.1.0            | 4.0.0           |
| xunit.v3								  | 3.2.2            | 3.2.2           |

## [6.0.0] - 19.05-2026

### Changes

- Updated [TestingStoreManager] to handle sanitized data for integration testing iterations.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.1.0            | 4.0.0           |
| xunit.v3								  | 3.2.2            | 3.2.2           |

## [5.1.0] - 16.03-2026

### Changes

- Now testing base methods names have a better description of the test itself.
- Now [Update_Success] tests is required to be implemented since there're special checks.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.1.0            | 2.1.0           |
| xunit.v3								  | 3.2.2            | 3.2.2           |

## [5.0.1] - 16.03-2026

### Changed

- Exposed package documentation.
- Added solution documentation.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.1.0            | 2.1.0           |
| xunit.v3								  | 3.2.2            | 3.2.2           |

## [5.0.0] - 15.02-2026

### Changed

- Removed [xunit] and [SkippableFact] packages for [xunit.v3] upgrade.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.1.0            | 2.1.0           |
| xunit.v3								  | -.-.-            | 3.2.2           |
| xunit									  | x.x.x            | x.x.x           |

## [4.1.1] - 24.12-2025

### Changed

- Updated packages and framework.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.0.0            | 2.1.0           |
| xunit									  | 2.9.3            | 2.9.3           |
| xunit.SkippableFact   				  | 1.5.23           | 1.5.23          |

## [4.1.1] - 22.12-2025

### Changed

- Updated packages and framework.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.0.0            | 2.0.0           |
| xunit									  | 2.9.3            | 2.9.3           |
| xunit.SkippableFact   				  | 1.5.23           | 1.5.23          |

## [4.1.0] - 16.12-2025

### Added

- Added new [BaseDraftUtils] to draft entities based on their base classes, simplifying data mocking.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 2.0.0            | 2.0.0           |
| xunit									  | 2.9.3            | 2.9.3           |
| xunit.SkippableFact   				  | 1.5.23           | 1.5.23          |

## [4.0.0] - 11.12-2025

### Changed

- Reverted and removed version [3.0.0], the interface are not needed in this context abstraction level

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 1.3.1            | 2.0.0           |
| xunit									  | 2.9.3            | 2.9.3           |
| xunit.SkippableFact   				  | 1.5.23           | 1.5.23          |

## [3.0.0] - 11.12-2025

### Changed

- Changed [TestDepotBase] to handle correctly interfacing for operations.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 1.3.1            | 2.0.0           |
| xunit									  | 2.9.3            | 2.9.3           |
| xunit.SkippableFact   				  | 1.5.23           | 1.5.23          |

## [2.1.0] - 26.11-2025

### Changed

- Added database activation for testing purposes.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 1.3.1            | 1.3.0           |
| xunit									  | 2.9.3            | 2.9.3           |
| xunit.SkippableFact   				  | 1.5.23           | 1.5.23          |

## [2.0.2] - 25.11-2025

### Changed

- Removed unnecessary param from [TestingDepotBase] Sign.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 1.3.1            | 1.3.0           |
| xunit									  | 2.9.3            | 2.9.3           |
| xunit.SkippableFact   				  | 1.5.23           | 1.5.23          |

## [2.0.1] - 25.11-2025

### Changed

- Simplified access for testing database activation using signature property.

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 1.3.1            | 1.3.0           |
| xunit									  | 2.9.3            | 2.9.3           |
| xunit.SkippableFact   				  | 1.5.23           | 1.5.23          |

## [2.0.0] - 24.11-2025

### Added

- Package initialization.

### Changed

- Renaming of abstractions and organization.
- Renaming of [Q_EntityValidations] to [TestingEntityBase] 

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | 1.2.1            | 1.3.0           |
| xunit									  | 2.9.3            | 2.9.3           |
| xunit.SkippableFact   				  | --.--.--         | 1.5.23          |


## [1.0.0] - 06.08-2025

### Added

- Package initialization.

### Fixed

### Changed

#### Dependencies

| Package                                 | Previous Version | New Version     |
|:----------------------------------------|:----------------:|:---------------:|
| CSM.Foundation.Core                     | --.--.--         | 1.2.1           |
| xunit									  | --.--.--         | 2.9.3           |