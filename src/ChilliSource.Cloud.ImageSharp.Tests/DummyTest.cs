using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ChilliSource.Cloud.ImageSharp.Tests
{
    public class DummyTest
    {

        [Fact]
        public void PassTest()
        {
            Assert.True(1 == 1);
        }
    }
}
