using CustomBlockChainLab.Models;
using CustomBlockChainLab.Models.Domains;
using CustomBlockChainLab.Repositories.Interfaces;
using CustomBlockChainLab.Services;
using FluentAssertions;
using NSubstitute;

namespace CustomBlockChainLabUnitTests.Services;

[TestFixture]
public class ChainServiceTests
{
    private IChainRepository? _chainRepository;
    private ChainService _chainService;

    [SetUp]
    public void SetUp()
    {
        _chainRepository = Substitute.For<IChainRepository>();
        _chainService = new ChainService(_chainRepository);
    }

    [Test]
    public void should_insert_new_block_to_chain()
    {
        GivenBlock(new BlockDomain
        {
            Hash = "123"
        });

        _chainService.GenerateNewBlock(new GenerateNewBlockDto
        {
            Data = "",
            TimeStamp = DateTime.Now
        });

        _chainRepository.Received()!.InsertBlock(Arg.Any<BlockDomain>());
    }

    [Test]
    public async Task should_not_request_block_when_the_length_is_0()
    {
        _chainRepository!.GetChainLength().Returns(0);
        await _chainService.GenerateNewBlock(new GenerateNewBlockDto()
        {
            Data = "new",
            TimeStamp = DateTime.Now
        });

        _chainRepository.DidNotReceive().GetBlockBy(Arg.Any<int>());
        await _chainRepository.Received().InsertBlock(Arg.Is<BlockDomain>(b => b.Data == "Genesis Block"));
    }

    [Test]
    public async Task should_generate_new_block_base_on_latest_block()
    {
        _chainRepository!.GetChainLength().Returns(1);

        GivenBlock(new BlockDomain
        {
            Hash = "123",
        });

        var newBlock = await _chainService.GenerateNewBlock(new GenerateNewBlockDto()
        {
            Data = "new",
            TimeStamp = DateTime.Now
        });

        _chainRepository.Received().GetBlockBy(Arg.Is<int>(i => i==1-1));
        newBlock.PreviousHash.Should().Be("123");
    }

    private void GivenBlock(BlockDomain blockDomain)
    {
        _chainRepository?.GetBlockBy(Arg.Any<int>()).Returns(blockDomain);
    }
}