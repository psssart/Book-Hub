using App.DAL.EF.Repositories;
using AutoMapper;
using Base.DAL.EF;
using Base.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Base.Tests.DAL;

public class BaseRepositoryTest
{
    private readonly TestDbContext _ctx;
    private readonly TestEntityRepository _testEntityRepository;

    public BaseRepositoryTest()
    {
        // set up mock database - inmemory
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();

        // use random guid as db instance id
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        _ctx = new TestDbContext(optionsBuilder.Options);

        // reset db
        _ctx.Database.EnsureDeleted();
        _ctx.Database.EnsureCreated();

        var config = new MapperConfiguration(cfg => cfg.CreateMap<TestEntity, TestEntity>());
        var mapper = config.CreateMapper();

        _testEntityRepository =
            new TestEntityRepository(
                _ctx,
                new BaseDalDomainMapper<TestEntity, TestEntity>(mapper)
            );
    }


    [Fact]
    public async Task Test1()
    {
        // arrange
        _testEntityRepository.Add(new TestEntity() {Value = "Foo"});
        _ctx.SaveChanges();

        // act
        var data = await _testEntityRepository.GetAllAsync();

        // assert
        Assert.Equal(1, data.Count());
    }
}