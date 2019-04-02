using System.Collections.Generic;
using System.Diagnostics;
using AElf.BenchBase;
using AElf.Kernel.Blockchain.Application;
using AElf.Kernel.Blockchain.Domain;
using AElf.Kernel.SmartContractExecution.Application;
using AElf.OS;
using NBench;
using Pro.NBench.xUnit.XunitExtensions;
using Volo.Abp.Threading;
using Xunit.Abstractions;

namespace AElf.Kernel.SmartContractExecution.Benches
{
    public class FullBlockchainExecutingTests: BenchBaseTest<ExecutionBenchAElfModule>
    {
        private IBlockchainService _blockchainService;
        private IBlockchainExecutingService _blockchainExecutingService;
        private IBlockExecutingService _blockExecutingService;
        private IChainManager _chainManager;
        private OSTestHelper _osTestHelper;
        
        private Counter _counter;

        private Block _block;
        
        public FullBlockchainExecutingTests(ITestOutputHelper output)
        {
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new XunitTraceListener(output));
        }
        
        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _blockchainService = GetRequiredService<IBlockchainService>();
            _blockchainExecutingService = GetRequiredService<IBlockchainExecutingService>();
            _blockExecutingService = GetRequiredService<IBlockExecutingService>();
            _chainManager = GetRequiredService<IChainManager>();
            _osTestHelper = GetRequiredService<OSTestHelper>();
            
            _counter = context.GetCounter("TestCounter");

            AsyncHelper.RunSync(async () =>
            {
                var chain = await _blockchainService.GetChainAsync();

                var transactions = new List<Transaction>();
                for (int i = 0; i < 1000; i++)
                {
                    var transaction = await _osTestHelper.GenerateTransferTransaction();
                    transactions.Add(transaction);
                }

                _block = _osTestHelper.GenerateBlock(chain.BestChainHash, chain.BestChainHeight, transactions);
                
                await _blockchainService.AddBlockAsync(_block);
                chain = await _blockchainService.GetChainAsync();
                await _blockchainService.AttachBlockToChainAsync(chain, _block);
            });
        }
        
        [NBenchFact]
        [PerfBenchmark(NumberOfIterations = 5, RunMode = RunMode.Iterations,
            RunTimeMilliseconds = 1000, TestMode = TestMode.Test)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, .0d)]
        public void ExecuteBlocksAttachedToLongestChainTest()
        {
            AsyncHelper.RunSync(async () =>
            {
                var chain = await _blockchainService.GetChainAsync();
                await _blockchainExecutingService.ExecuteBlocksAttachedToLongestChain(chain,
                    BlockAttachOperationStatus.LongestChainFound);
            });
            _counter.Increment();
        }
        
        [NBenchFact]
        [PerfBenchmark(NumberOfIterations = 5, RunMode = RunMode.Iterations,
            RunTimeMilliseconds = 1000, TestMode = TestMode.Test)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, .0d)]
        public void GetNotExecutedBlocksTest()
        {
            AsyncHelper.RunSync(async () =>
            {
                var chain = await _blockchainService.GetChainAsync();
                await _chainManager.GetNotExecutedBlocks(chain.LongestChainHash);
            });
            _counter.Increment();
        }
        
        [NBenchFact]
        [PerfBenchmark(NumberOfIterations = 5, RunMode = RunMode.Iterations,
            RunTimeMilliseconds = 1000, TestMode = TestMode.Test)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, .0d)]
        public void GetBlockByHashTest()
        {
            AsyncHelper.RunSync(() => _blockchainService.GetBlockByHashAsync(_block.GetHash()));
            _counter.Increment();
        }
        
        [NBenchFact]
        [PerfBenchmark(NumberOfIterations = 5, RunMode = RunMode.Iterations,
            RunTimeMilliseconds = 1000, TestMode = TestMode.Test)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, .0d)]
        public void ExecuteBlock()
        {
            AsyncHelper.RunSync(async () =>
            {
                var chain = await _blockchainService.GetChainAsync();
                //var blockLinks = await _chainManager.GetNotExecutedBlocks(chain.LongestChainHash);
                var blockLink = await _chainManager.GetChainBlockLinkAsync(_block.GetHash());
                var linkedBlock = await _blockchainService.GetBlockByHashAsync(blockLink.BlockHash);
                await _blockExecutingService.ExecuteBlockAsync(linkedBlock.Header, linkedBlock.Body.TransactionList);
                await _chainManager.SetChainBlockLinkExecutionStatus(blockLink, ChainBlockLinkExecutionStatus.ExecutionSuccess);
                
                await _blockchainService.SetBestChainAsync(chain, blockLink.Height, blockLink.BlockHash);
            });
            _counter.Increment();
        }

        [NBenchFact]
        [PerfBenchmark(NumberOfIterations = 5, RunMode = RunMode.Iterations,
            RunTimeMilliseconds = 1000, TestMode = TestMode.Test)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, .0d)]
        public void SetBestChainTest()
        {
            AsyncHelper.RunSync(async () =>
            {
                var chain = await _blockchainService.GetChainAsync(); 
                
                await _blockchainService.SetBestChainAsync(chain, _block.Height, _block.GetHash());
            });
            _counter.Increment();
        }
    }
}