﻿using System.Collections.Generic;

namespace ModShardLauncher.ModUtils;

public class DE2Dialogue
{
    public string Name { get; set; }
    public List<DE2Topic> DialogueTopics { get; set; }
        
    public DE2Dialogue(string name)
    {
        this.Name = name;
        this.DialogueTopics = new List<DE2Topic>();
    }
        
    public DE2Dialogue AddTopic(params DE2Topic[] topics)
    {
        foreach (DE2Topic topic in topics)
            this.DialogueTopics.Add(topic);
        return this;
    }
}
    
public class DE2Topic
{
    public string Name { get; set; }
    public List<DE2Option> TopicOptions { get; set; }
        
    public DE2Topic(string name, params DE2Option[] topicOptions)
    {
        this.Name = name;
        this.TopicOptions = new List<DE2Option>(topicOptions);
    }
        
    public DE2Topic AddOption(params DE2Option[] options)
    {
        foreach (DE2Option option in options)
            this.TopicOptions.Add(option);
        return this;
    }
}

public class DE2Option
{
    public string? Text { get; set; }
    public string? Script { get; set; }
    public string? NextTopic { get; set; }

        
    public DE2Option(string? text, string? script, string? nextTopic)
    {
        this.Text = text;
        this.Script = script;
        this.NextTopic = nextTopic;
    }
}