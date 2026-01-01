using FluentAssertions;
using NUnit.Framework;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Tests.ServiceImplementation;

namespace SecureFolderFS.Tests.ServiceTests
{
    [TestFixture]
    public class LocalizationServiceTests
    {
        private MockLocalizationService _localizationService = null!;

        [SetUp]
        public void SetUp()
        {
            _localizationService = new MockLocalizationService();
        }

        [Test]
        public void LocalizeDate_UnspecifiedDate_ReturnsUnspecified()
        {
            // Arrange
            var date = new DateTime(1, 1, 1);

            // Act
            var result = _localizationService.LocalizeDate(date);

            // Assert
            result.Should().Be("Unspecified");
        }

        [Test]
        public void LocalizeDate_Today_ReturnsFormattedToday()
        {
            // Arrange
            var date = DateTime.Today.AddHours(14).AddMinutes(30);
            var expectedTime = date.ToString("t", _localizationService.CurrentCulture);

            // Act
            var result = _localizationService.LocalizeDate(date);

            // Assert
            result.Should().Be($"Today, {expectedTime}");
        }

        [Test]
        public void LocalizeDate_Yesterday_ReturnsFormattedYesterday()
        {
            // Arrange
            var date = DateTime.Today.AddDays(-1).AddHours(10).AddMinutes(15);
            var expectedTime = date.ToString("t", _localizationService.CurrentCulture);

            // Act
            var result = _localizationService.LocalizeDate(date);

            // Assert
            result.Should().Be($"Yesterday, {expectedTime}");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void LocalizeDate_TwoToSixDaysAgo_ReturnsDaysAgo(int daysAgo)
        {
            // Arrange
            var date = DateTime.Today.AddDays(-daysAgo);

            // Act
            var result = _localizationService.LocalizeDate(date);

            // Assert
            result.Should().Be($"{daysAgo} days ago");
        }

        [TestCase(7)]
        [TestCase(10)]
        [TestCase(13)]
        public void LocalizeDate_SevenToThirteenDaysAgo_ReturnsLastWeek(int daysAgo)
        {
            // Arrange
            var date = DateTime.Today.AddDays(-daysAgo);

            // Act
            var result = _localizationService.LocalizeDate(date);

            // Assert
            result.Should().Be("Last week");
        }

        [TestCase(14)]
        [TestCase(30)]
        [TestCase(365)]
        public void LocalizeDate_FourteenOrMoreDaysAgo_ReturnsFallbackFormat(int daysAgo)
        {
            // Arrange
            var date = DateTime.Today.AddDays(-daysAgo).AddHours(9).AddMinutes(45);
            var expectedDate = date.ToString("d", _localizationService.CurrentCulture);
            var expectedTime = date.ToString("t", _localizationService.CurrentCulture);

            // Act
            var result = _localizationService.LocalizeDate(date);

            // Assert
            result.Should().Be($"{expectedDate}, {expectedTime}");
        }
    }
}