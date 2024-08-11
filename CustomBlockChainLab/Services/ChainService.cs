using CustomBlockChainLab.Models;
using CustomBlockChainLab.Models.Domains;
using CustomBlockChainLab.Repositories.Interfaces;
using CustomBlockChainLab.Services.Interfaces;

namespace CustomBlockChainLab.Services;

public class ChainService(IChainRepository chainRepository) : IChainService
{
    private const int Nonce = 0;

    public async Task<BlockDomain> GetBlockById(int i)
    {
        return await chainRepository.GetBlockBy(i);
    }

    public async Task<BlockDomain> GenerateNewBlock(GenerateNewBlockDto dto)
    {
        var chainLength = await chainRepository.GetChainLength();
        var newBlock = chainLength == 0
            ? new BlockDomain
            {
                Data = "Genesis Block",
                Hash = "0",
                PreviousHash = "0",
                TimeStamp = DateTime.Now,
                Nonce = 0
            }
            : (await chainRepository.GetBlockBy(chainLength)).GenerateNextBlock(dto, Nonce);

        await chainRepository.InsertBlock(newBlock);
        return newBlock;
    }
}