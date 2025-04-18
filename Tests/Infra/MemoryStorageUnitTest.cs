using System.Collections.Concurrent;
using Infra;

namespace Tests.Infra;

public class MemoryStorageTests
{
    private readonly MemoryStorage _storage = new();

    [Fact]
    public void Set_ShouldStoreValue_WhenKeyIsValid()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";
        
        // Act
        _storage.Set(key, value);
        
        // Assert
        Assert.Equal(value, _storage.Get<string>(key));
    }

    [Fact]
    public void Set_ShouldOverwriteValue_WhenKeyAlreadyExists()
    {
        // Arrange
        var key = "existingKey";
        var initialValue = "initialValue";
        var newValue = "newValue";
        
        _storage.Set(key, initialValue);
        
        // Act
        _storage.Set(key, newValue);
        
        // Assert
        Assert.Equal(newValue, _storage.Get<string>(key));
    }

    [Fact]
    public void Get_ShouldReturnDefault_WhenKeyDoesNotExist()
    {
        // Arrange
        var nonExistentKey = "nonExistentKey";
        
        // Act
        var result = _storage.Get<string>(nonExistentKey);
        
        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public void Get_ShouldReturnCorrectType_WhenValueExists()
    {
        // Arrange
        var key = "typedKey";
        var intValue = 42;
        _storage.Set(key, intValue);
        
        // Act
        var result = _storage.Get<int>(key);
        
        // Assert
        Assert.IsType<int>(result);
        Assert.Equal(intValue, result);
    }

    [Fact]
    public void Remove_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        var key = "keyToRemove";
        _storage.Set(key, "value");
        
        // Act
        var result = _storage.Remove(key);
        
        // Assert
        Assert.True(result);
        Assert.False(_storage.ContainsKey(key));
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var nonExistentKey = "nonExistentKey";
        
        // Act
        var result = _storage.Remove(nonExistentKey);
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ContainsKey_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        var key = "existingKey";
        _storage.Set(key, "value");
        
        // Act
        var result = _storage.ContainsKey(key);
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsKey_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var nonExistentKey = "nonExistentKey";
        
        // Act
        var result = _storage.ContainsKey(nonExistentKey);
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Set_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var key = "concurrentKey";
        var values = new[] { "value1", "value2", "value3" };
        
        // Act
        Parallel.ForEach(values, value =>
        {
            _storage.Set(key, value);
        });
        
        // Assert
        var result = _storage.Get<string>(key);
        Assert.Contains(result, values);
    }

    [Fact]
    public void Get_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var key = "concurrentReadKey";
        var value = "testValue";
        _storage.Set(key, value);
        var results = new ConcurrentBag<string>();
        
        // Act
        Parallel.For(0, 100, _ =>
        {
            results.Add(_storage.Get<string>(key));
        });
        
        // Assert
        Assert.All(results, r => Assert.Equal(value, r));
    }

    [Fact]
    public void Remove_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var key = "concurrentRemoveKey";
        _storage.Set(key, "value");
        var results = new ConcurrentBag<bool>();
        
        // Act
        Parallel.For(0, 100, _ =>
        {
            results.Add(_storage.Remove(key));
        });
        
        // Assert
        // Only one remove should return true, others should return false
        Assert.Single(results.Where(r => r));
    }

    [Fact]
    public void Set_ShouldHandleNullValues()
    {
        // Arrange
        var key = "nullValueKey";
        
        // Act
        _storage.Set<string>(key, null);
        
        // Assert
        Assert.Null(_storage.Get<string>(key));
    }

    [Fact]
    public void Get_ShouldReturnCorrectValue_ForDifferentTypes()
    {
        // Arrange
        var stringKey = "stringKey";
        var stringValue = "stringValue";
        
        var intKey = "intKey";
        var intValue = 123;
        
        var objectKey = "objectKey";
        var objectValue = new { Name = "Test", Value = 42 };
        
        _storage.Set(stringKey, stringValue);
        _storage.Set(intKey, intValue);
        _storage.Set(objectKey, objectValue);
        
        // Act & Assert
        Assert.Equal((string?)stringValue, _storage.Get<string>(stringKey));
        Assert.Equal(intValue, _storage.Get<int>(intKey));
        Assert.Equal(objectValue, _storage.Get<object>(objectKey));
    }

    [Fact]
    public void Storage_ShouldBeThreadSafe_ForMultipleOperations()
    {
        // Arrange
        var testKey = "threadSafeKey";
        var initialValue = "initial";
        _storage.Set(testKey, initialValue);
        
        // Act
        Parallel.For((long)0, 1000, i =>
        {
            switch (i % 4)
            {
                case 0:
                    _storage.Set(testKey, $"value{i}");
                    break;
                case 1:
                    _ = _storage.Get<string>(testKey);
                    break;
                case 2:
                    _ = _storage.ContainsKey(testKey);
                    break;
                case 3:
                    _ = _storage.Remove(testKey);
                    break;
            }
        });
        
        // Assert
        // No exception should be thrown
        Assert.True(true);
    }
}