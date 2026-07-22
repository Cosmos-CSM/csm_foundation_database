using CSM_Database_Core.Core.Utils;

using DatabaseProxy.Entities;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace UnitTests.Tests.UtilsTests;


/// <summary>
///     Unit tests class for <see cref="DatabaseUtils"/>.
/// </summary>
public class DatabaseUtilsTests {


    /// <summary>
    ///     Method: <see cref="DatabaseUtils.SanitizeEntity{TEntity}(DbContext, TEntity)"/> 
    ///     Expectation: Entity dependencies don´t throw relation detection exceptions.
    /// </summary>
    [Fact]
    public async Task SatinizeEntity_RelationDetection() {

        //Mocking
        Mock<DbContext> databaseProxyMock = new();
        Mock<EntityProxy> entityProxyMock = new();

        //Acting
        EntityProxy output = await DatabaseUtils.SanitizeEntity(databaseProxyMock.Object, entityProxyMock.Object);

        //Asserting
        Assert.NotNull(output);
    }
}
