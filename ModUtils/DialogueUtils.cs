using System.Collections.Generic;

namespace ModShardLauncher.ModUtils
{
    public class DE2Dialogue
    {
        public string Name { get; set; }
        public List<DE2Topic> DialogueTopics { get; set; }
        
        public DE2Dialogue(string name, List<DE2Topic> dialogueTopics)
        {
            this.Name = name;
            this.DialogueTopics = dialogueTopics;
        }
        
        public DE2Dialogue AddTopic(DE2Topic topic)
        {
            this.DialogueTopics.Add(topic);
            return this;
        }
    }
    
    public class DE2Topic
    {
        public string Name { get; set; }
        private List<DE2Option> TopicOptions { get; set; }
        
        public DE2Topic(string name, List<DE2Option> topicOptions)
        {
            this.Name = name;
            this.TopicOptions = topicOptions;
        }
        
        public DE2Topic AddOption(DE2Option option)
        {
            this.TopicOptions.Add(option);
            return this;
        }
    }

    public class DE2Option
    {
        public string? Text { get; set; }
        public string? Script { get; set; }
        public string NextTopic { get; set; }
        
        public DE2Option(string text, string script, string nextTopic)
        {
            this.Text = text;
            this.Script = script;
            this.NextTopic = nextTopic;
        }
    }

    public static partial class Msl
    {
        
    }
}