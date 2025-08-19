# Test Coverage Analysis for LinqToDB.MigrateUp

## Current Test Coverage

### ✅ What's Well Tested

#### 1. **Basic Migration Flow**
- Migration can run successfully
- Migration with caching can run multiple times
- RunForEntity works with specific entities
- Mixed profiles execution
- State tracking (created tables/indexes)

#### 2. **Caching**
- InMemoryMigrationCache operations
- Task hashing for cache keys
- Cache enable/disable scenarios
- Skip cached migrations

#### 3. **Provider Base**
- GetEntityColumns returns correct columns
- UpdateTableSchema creates table when missing
- UpdateTableSchema adds missing columns
- UpdateTableSchema alters columns when types differ (basic test)
- EnsureIndex creates/replaces indexes
- Index validation (empty name, empty columns)

#### 4. **Expressions**
- CreateTableExpression execution
- CreateIndexExpression with various configurations
- DataImportExpression with conditions (WhenTableEmpty, WhenTableCreated)

---

## 🔴 Testing Gaps Identified

### 1. **Column Alteration Scenarios - CRITICAL GAP**
Currently only basic test exists. Missing:
- [ ] Nullable to non-nullable column changes
- [ ] Non-nullable to nullable column changes
- [ ] Data type changes (INT to BIGINT, VARCHAR(50) to VARCHAR(100))
- [ ] Column with default values
- [ ] Column with constraints (UNIQUE, CHECK)
- [ ] Column rename operations
- [ ] Columns with foreign keys
- [ ] **SQLite limitation handling** (ALTER COLUMN not fully supported)
- [ ] Data preservation during alterations
- [ ] Rollback scenarios when alteration fails

### 2. **Error Handling & Edge Cases**
- [ ] Table/column names with special characters
- [ ] SQL injection prevention in identifiers
- [ ] Very long table/column names
- [ ] Reserved SQL keywords as names
- [ ] Concurrent migration execution
- [ ] Network failures during migration
- [ ] Partial migration failure recovery
- [ ] Invalid data types in column definitions
- [ ] Circular dependencies in migrations

### 3. **Provider-Specific Behavior**
- [ ] SQLite-specific limitations (no ALTER COLUMN)
- [ ] SQL Server-specific features (schemas, computed columns)
- [ ] PostgreSQL-specific types (arrays, JSON)
- [ ] MySQL-specific behavior
- [ ] Cross-database type mapping

### 4. **Data Import Scenarios**
- [ ] Large dataset imports (performance)
- [ ] Import with duplicate key handling
- [ ] Import with foreign key violations
- [ ] Import with data type mismatches
- [ ] Partial import failures
- [ ] Transaction handling during imports

### 5. **Index Management**
- [ ] Composite indexes with 3+ columns
- [ ] Unique indexes
- [ ] Filtered/partial indexes
- [ ] Clustered vs non-clustered indexes
- [ ] Index with included columns
- [ ] Index name conflicts

### 6. **Schema Detection**
- [ ] GetTableColumns with no columns
- [ ] GetTableColumns with system columns
- [ ] Tables in different schemas (SQL Server)
- [ ] Temporary tables
- [ ] Views vs tables distinction

### 7. **Integration Tests**
- [ ] Full migration with real SQLite database
- [ ] Full migration with SQL Server (if available)
- [ ] Migration rollback scenarios
- [ ] Migration with multiple databases
- [ ] Migration versioning

### 8. **Performance Tests**
- [ ] Large number of tables (100+)
- [ ] Large number of columns per table (50+)
- [ ] Large data imports (10k+ rows)
- [ ] Migration execution time tracking

### 9. **Configuration & Validation**
- [ ] Invalid migration configurations
- [ ] Circular profile dependencies
- [ ] Profile with no tasks
- [ ] Duplicate entity types in profiles
- [ ] Options validation edge cases

### 10. **Query Service**
- [ ] SqlQueryResult with different providers
- [ ] Query generation for edge cases
- [ ] Query injection prevention
- [ ] Query performance

---

## Recommended Test Priorities

### Priority 1 - Critical Gaps
1. **Column Alteration Test Suite** - Most important gap
2. **SQLite ALTER COLUMN limitation handling**
3. **Error recovery and partial failure scenarios**

### Priority 2 - Important
1. **Provider-specific integration tests**
2. **Data import error scenarios**
3. **Schema detection edge cases**

### Priority 3 - Nice to Have
1. **Performance benchmarks**
2. **Cross-database compatibility**
3. **Advanced index scenarios**

---

## Test Implementation Plan

### Phase 1: Column Alteration Tests
```csharp
[TestFixture]
public class ColumnAlterationTests
{
    [Test]
    public void AlterColumn_NullableToNonNullable_PreservesData()
    
    [Test]
    public void AlterColumn_DataTypeChange_HandlesConversion()
    
    [Test]
    public void AlterColumn_WithDefaultValue_AppliesDefault()
    
    [Test]
    public void AlterColumn_SqliteLimitation_RecreatesTable()
}
```

### Phase 2: Error Handling Tests
```csharp
[TestFixture]
public class MigrationErrorHandlingTests
{
    [Test]
    public void Migration_WithInvalidIdentifier_ThrowsMeaningfulError()
    
    [Test]
    public void Migration_PartialFailure_RollsBackTransaction()
    
    [Test]
    public void Migration_ConcurrentExecution_HandlesLocking()
}
```

### Phase 3: Integration Tests
```csharp
[TestFixture]
public class SqliteIntegrationTests
{
    [Test]
    public void FullMigration_WithRealDatabase_Succeeds()
    
    [Test]
    public void AlterColumn_InSqlite_RecreatesTableCorrectly()
}
```

---

## Code Coverage Metrics

**Current Estimated Coverage:**
- Core Logic: ~70%
- Error Paths: ~30%
- Provider-Specific: ~40%
- Edge Cases: ~20%

**Target Coverage:**
- Core Logic: 90%
- Error Paths: 80%
- Provider-Specific: 70%
- Edge Cases: 60%