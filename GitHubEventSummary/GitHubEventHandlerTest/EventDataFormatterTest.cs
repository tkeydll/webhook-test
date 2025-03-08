using GitHubEventHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace GitHubEventHandlerTest
{
    [TestClass]
    public sealed class EventDataFormatterTest
    {
        private string _testDataPath = "../../../data/test/";
        private string _expectedDataPath = "../../../data/expected/";

        [TestMethod]
        [DataRow("edit_issue.json")]
        [DataRow("issue_comment.json")]
        // [DataRow("pull_request.json")]
        // [DataRow("pull_request_comment.json")]
        // [DataRow("push.json")]
        public void TestFormatEventData(string fileName)
        {
            // Arrange
            string jsonData = System.IO.File.ReadAllText($"{_testDataPath}{fileName}");
            string expected = System.IO.File.ReadAllText($"{_expectedDataPath}ex_{fileName}");

            // Act
            string actual = EventDataFormatter.FormatEventData(jsonData);

            // Assert
            JObject expectedJObject = JObject.Parse(expected);
            JObject actualJObject = JObject.Parse(actual);

            Assert.AreEqual(expectedJObject.ToString(), actualJObject.ToString());
        }
    }
}
