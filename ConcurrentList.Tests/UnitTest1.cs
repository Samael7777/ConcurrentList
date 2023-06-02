using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ConcurrentList.Tests;

[TestFixture]
public class Tests
{
    private const int numOfThreads = 100;
    private const int numOfItemsPerThread = 5;
    private ConcurrentList<string> _concurrentList = null!;
    private List<Task> _tasks = null!;

    [SetUp]
    public void Setup()
    {
        _concurrentList = new ConcurrentList<string>();
        _tasks = new List<Task>();
    }

    [TearDown]
    public void TearDown()
    {
        _tasks.Clear();
        _concurrentList.Clear();
    }

    [Test]
    public void MultiThreadReading()
    {
        _concurrentList.Add("First");
        _concurrentList.Add("Second");
        _concurrentList.Add("Third");

        for(var i = 0; i < numOfThreads; i++)
        {
            var task = Task.Run(ReadTask);
            _tasks.Add(task);
        }

        Task.WaitAll(_tasks.ToArray());
        Assert.Pass();
    }

    [Test]
    public void MultiThreadWriting()
    {
        for(var i = 0; i < numOfThreads; i++)
        {
            var id = i.ToString();
            var task = Task.Run(() =>
            {
                WriteTask(id);
                
            });
            _tasks.Add(task);
        }

        Task.WaitAll(_tasks.ToArray());
        Assert.AreEqual(numOfThreads * numOfItemsPerThread ,_concurrentList.Count);
    }

    [Test]
    public void MultiThreadReadAndWrite()
    {
        for(var i = 0; i < numOfThreads; i++)
        {
            var id = i.ToString();
            var task = i % 2 == 0
                ? Task.Run(ReadTask)
                : Task.Run(() => { WriteTask(id); });
            _tasks.Add(task);
        }
        Task.WaitAll(_tasks.ToArray());
        Assert.Pass();
    }

    private void ReadTask()
    {
        try
        {
            foreach (var item in _concurrentList)
            {
                _ = item;
            }
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
        
    }

    private void WriteTask(string data)
    {
        try
        {
            for (var j = 0; j < numOfItemsPerThread; j++)
            {
                var item = $"{data}_{j}";
                _concurrentList.Add(item);
            }
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}