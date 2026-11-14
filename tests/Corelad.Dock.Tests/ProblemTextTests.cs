using System;
using Corelad.Dock.Diagnostics;
using Corelad.Geometry.Diagnostics;
using Xunit;

namespace Corelad.Dock.Tests
{
    /// <summary>
    /// ProblemText: kod → Türkçe başlık/açıklama ve önem → etiket/simge eşlemesi.
    /// Metinler kullanıcıya gösterilir; boş veya İngilizce kalmamalıdır.
    /// </summary>
    public sealed class ProblemTextTests
    {
        [Fact]
        public void AllCodes_HaveTurkishTitleAndDescription()
        {
            foreach (ProblemCode code in Enum.GetValues(typeof(ProblemCode)))
            {
                Assert.False(string.IsNullOrWhiteSpace(ProblemText.Title(code)), "Başlık boş: " + code);
                Assert.False(
                    string.IsNullOrWhiteSpace(ProblemText.Description(code)),
                    "Açıklama boş: " + code);
                Assert.NotEqual("Bilinmeyen sorun", ProblemText.Title(code));
            }
        }

        [Theory]
        [InlineData(ProblemCode.OpenContour, "Kontur kapalı değil")]
        [InlineData(ProblemCode.CrossingContours, "Konturlar kesişiyor")]
        [InlineData(ProblemCode.UnsupportedObject, "Desteklenmeyen nesne")]
        [InlineData(ProblemCode.NestedIsland, "Delik içinde ayrı gövde")]
        [InlineData(ProblemCode.WriteValidationFailed, "Yazma doğrulaması başarısız")]
        public void Title_MapsKnownCodesToTurkish(ProblemCode code, string expected)
        {
            Assert.Equal(expected, ProblemText.Title(code));
        }

        [Fact]
        public void UnsupportedObject_DescriptionCoversGroupAndPowerClipCases()
        {
            var description = ProblemText.Description(ProblemCode.UnsupportedObject);

            Assert.Contains("Grup", description, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("PowerClip", description, StringComparison.Ordinal);
            Assert.Contains("cdrCurveShape", description, StringComparison.Ordinal);
        }

        [Fact]
        public void UnknownCode_FallsBackToTurkishPlaceholder()
        {
            var code = (ProblemCode)999;

            Assert.Equal("Bilinmeyen sorun", ProblemText.Title(code));
            Assert.Contains("Tanımlanmamış", ProblemText.Description(code), StringComparison.Ordinal);
        }

        [Theory]
        [InlineData(ProblemSeverity.Error, "Hata", "×")]
        [InlineData(ProblemSeverity.Warning, "Uyarı", "!")]
        [InlineData(ProblemSeverity.Info, "Bilgi", "i")]
        public void Severity_MapsToTurkishLabelAndGlyph(
            ProblemSeverity severity,
            string expectedLabel,
            string expectedGlyph)
        {
            Assert.Equal(expectedLabel, ProblemText.SeverityLabel(severity));
            Assert.Equal(expectedGlyph, ProblemText.SeverityGlyph(severity));
        }

        [Fact]
        public void UnknownSeverity_FallsBackToInfoPresentation()
        {
            var severity = (ProblemSeverity)99;

            Assert.Equal("Bilgi", ProblemText.SeverityLabel(severity));
            Assert.Equal("i", ProblemText.SeverityGlyph(severity));
        }
    }
}
