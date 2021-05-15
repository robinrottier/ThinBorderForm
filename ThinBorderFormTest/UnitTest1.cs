using NUnit.Framework;
using rrSoft.Windows.Forms;

namespace ThinBorderFormTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [Test]
        public void TestPadding()
        {
            // create a form
            var f = new ThinBorderForm();
            // check padding at top is at least as high as the caption
            Assert.IsTrue(f.Padding.Top >= f.CaptionHeight);
            //
            // set the caption bigger and check padding changes too...
            int bigCaptionHeight = 50;
            f.CaptionHeight = bigCaptionHeight;
            Assert.IsTrue(f.CaptionHeight == bigCaptionHeight);
            Assert.IsTrue(f.Padding.Top >= bigCaptionHeight);
            //
            // set the padding smaller and check cant be smaller than caption
            var p = f.Padding;
            p.Top = 4;
            f.Padding = p;
            Assert.IsTrue(f.Padding.Top >= bigCaptionHeight);
        }
    }
}