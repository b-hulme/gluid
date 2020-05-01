using FluentAssertions;
using System;
using Xunit;

namespace Gluid.Tests
{
    /// <summary>
    /// Due to the use of Xor Hashing, regular unit testinng is quite difficult to achieve, as a result these tests use a slightly
    /// less conventional approach of round-tripping numbers into and out of Guid values
    /// </summary>
    public class GluidTests
    {
        public class RoundTrips
        {
            [Theory]
            [InlineData(1, "test", "test", 1)]
            [InlineData(2, "test", "blah", null)]
            [InlineData(3, "test", "", null)]
            [InlineData(4, "test", null, null)]
            [InlineData(5, "", "", 5)]
            [InlineData(6, "", null, null)]
            [InlineData(7, "", "blah", null)]
            [InlineData(8, null, null, 8)]
            [InlineData(9, null, "", null)]
            [InlineData(10, null, "blah", null)]
            [InlineData(int.MaxValue, "test", "test", int.MaxValue)]
            [InlineData(int.MaxValue, null, null, int.MaxValue)]
            [InlineData(int.MinValue, "test", "test", int.MinValue)]
            [InlineData(int.MinValue, null, null, int.MinValue)]
            public void ShouldReturnExpected_WhenUsingSingleInt(int input, string namespaceIn, string namespaceOut, int? expected)
            {
                // Arrange
                var guid = Gluid.NewGluid(input, namespaceIn);

                // Act
                var result = guid.ToInt32(namespaceOut);

                // Assert
                result.Should().Be(expected);
            }

            [Theory]
            [InlineData(10000000001, "test", "test", 10000000001)]
            [InlineData(10000000002, "test", "blah", null)]
            [InlineData(10000000003, "test", "", null)]
            [InlineData(10000000004, "test", null, null)]
            [InlineData(10000000005, "", "", 10000000005)]
            [InlineData(10000000006, "", null, null)]
            [InlineData(10000000007, "", "blah", null)]
            [InlineData(10000000008, null, null, 10000000008)]
            [InlineData(10000000009, null, "", null)]
            [InlineData(10000000010, null, "blah", null)]
            [InlineData(long.MaxValue, "test", "test", long.MaxValue)]
            [InlineData(long.MaxValue, null, null, long.MaxValue)]
            [InlineData(long.MinValue, "test", "test", long.MinValue)]
            [InlineData(long.MinValue, null, null, long.MinValue)]
            public void ShouldReturnExpected_WhenUsingSingleLong(long input, string namespaceIn, string namespaceOut, long? expected)
            {
                // Arrange
                var guid = Gluid.NewGluid(input, namespaceIn);

                // Act
                var result = guid.ToInt64(namespaceOut);

                // Assert
                result.Should().Be(expected);
            }

            [Theory]
            [InlineData(1, 10, "test", "test", 1, 10)]
            [InlineData(2, 9, "test", "blah", null, null)]
            [InlineData(3, 8, "test", "", null, null)]
            [InlineData(4, 7, "test", null, null, null)]
            [InlineData(5, 6, "", "", 5, 6)]
            [InlineData(6, 5, "", null, null, null)]
            [InlineData(7, 4, "", "blah", null, null)]
            [InlineData(8, 3, null, null, 8, 3)]
            [InlineData(9, 2, null, "", null, null)]
            [InlineData(10, 1, null, "blah", null, null)]
            [InlineData(int.MaxValue, int.MaxValue, "test", "test", int.MaxValue, int.MaxValue)]
            [InlineData(int.MaxValue, int.MaxValue, null, null, int.MaxValue, int.MaxValue)]
            [InlineData(int.MinValue, int.MinValue, "test", "test", int.MinValue, int.MinValue)]
            [InlineData(int.MinValue, int.MinValue, null, null, int.MinValue, int.MinValue)]
            [InlineData(int.MaxValue, int.MinValue, "test", "test", int.MaxValue, int.MinValue)]
            [InlineData(int.MaxValue, int.MinValue, null, null, int.MaxValue, int.MinValue)]
            [InlineData(int.MinValue, int.MaxValue, "test", "test", int.MinValue, int.MaxValue)]
            [InlineData(int.MinValue, int.MaxValue, null, null, int.MinValue, int.MaxValue)]
            public void ShouldReturnExpected_WhenUsingTwoInts(int input1, int input2, string namespaceIn, string namespaceOut, int? expected1, int? expected2)
            {
                // Arrange
                var guid = Gluid.NewGluid(input1, input2, namespaceIn);

                // Act
                var result1 = guid.ToInt32(namespaceOut);
                var result2 = guid.GetSecondInt32(namespaceOut);

                // Assert
                result1.Should().Be(expected1);
                result2.Should().Be(expected2);
            }
        }

        public class TheIsGluidMethod
        {
            [Fact]
            public void ShouldReturnTrue_WhenGuidIsAGluid()
            {
                // Arrange
                var guid = Gluid.NewGluid(1, "test");

                // Act
                var result = guid.IsGluid();

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public void ShouldReturnFalse_WhenGuidIsNotAGluid()
            {
                // Arrange
                var guid = Guid.NewGuid();

                // Act
                var result = guid.IsGluid();

                // Assert
                result.Should().BeFalse();
            }
        }

        public class TheIsLinkedMethod
        {
            [Theory]
            [InlineData("test", "test", true)]
            [InlineData("test", "blah", false)]
            [InlineData("test", "", false)]
            [InlineData("test", null, false)]
            [InlineData("", "", true)]
            [InlineData("", "blah", false)]
            [InlineData("", null, false)]
            [InlineData(null, null, true)]
            [InlineData(null, "", false)]
            [InlineData(null, "blah", false)]
            public void ShouldReturnExpected_WhenCuidIsAGluidAndNamespaceIsPassed(string namespaceIn, string namespaceOut, bool expected)
            {
                // Arrange
                var number = (new Random()).Next();
                var guid = Gluid.NewGluid(number, namespaceIn);

                // Act
                var result = guid.IsLinked(namespaceOut);

                // Assert
                result.Should().Be(expected);
            }

            [Theory]
            [InlineData("test")]
            [InlineData("")]
            [InlineData(null)]
            public void ShouldReturnFalse_WhenGuidIsNotAGluid(string @namespace)
            {
                // Arrange
                var guid = Guid.NewGuid();

                // Act
                var result = guid.IsLinked(@namespace);

                // Assert
                result.Should().BeFalse();
            }
        }
    }
}
