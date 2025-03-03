using GitHubEventHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace GitHubEventHandlerTest
{
    [TestClass]
    public sealed class EventDataFormatterTest
    {
        private string _testDataPath = "c:\\Users\\tkeyd\\git\\github-event\\GitHubEventSummary\\GitHubEventHandlerTest\\data\\test\\";
        private string _expectedDataPath = "c:\\Users\\tkeyd\\git\\github-event\\GitHubEventSummary\\GitHubEventHandlerTest\\data\\expected\\";

        [TestMethod]
        [DataRow("edit_issue.json")]
        [DataRow("issue_comment.json")]
        [DataRow("pull_request.json")]
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
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expected), JToken.Parse(actual)));
        }
    }
}
