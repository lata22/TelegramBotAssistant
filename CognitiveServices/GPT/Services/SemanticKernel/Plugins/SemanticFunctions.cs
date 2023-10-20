namespace CognitiveServices.AI.Services.SemanticKernel.Plugins;

public static class SemanticFunctions
{
    public static string TextChunkSummarizationPrompt(int tokenLimit)
        => "You are a text chunk summarizer.\n" +
        "The summary must always be in the same language as the text to summarize.\n" +
        "Summarize the following text within a limit of " + tokenLimit + " tokens:\n" +
        "Text to summarize:\n" +
        "{{$input}}";

    public static string ExtractAnswerFromJsonDataToAGivenUserPrompt(string userPrompt)
    {
        return
            "Based on the JSON data provided, your task is to extract a precise and relevant answer to the user's query: '" + userPrompt + "'\n" +
            "Your answer should meet the following criteria:\n" +
            "- It must strictly pertain to the user's query: '" + userPrompt + "'.\n" +
            "- If multiple pieces of information in the JSON data relate to the query, prioritize the most relevant and recent one.\n" +
            "- Format the answer clearly and concisely, eliminating any irrelevant details.\n" +
            "- Provide context or a brief explanation if the answer alone may not fully address the user's query.\n" +
            "If the JSON data doesn't contain information that can answer the user's query, explicitly state that no relevant information is available and answer with your knowledge in this particular case.\n" +
            "The answer must be in the same language as the user's query.\n" +
             "JSONDATA:\n" +
             "{{$input}}";
    }

    public const string NiceSummarizationJsonEmailsPrompt = @"Generate a clear and concise summary of the following JSON data containing unread emails.
    Your summary should include:
    - Total number of unread emails.
    - Sender's email, subject, and date for each email.
    - Indication of whether there are attachments or not.

    Use the following date format: YYYY-MM-DD HH:MM [AM/PM].
    Output should be organized and easy to read. 
    Keep your summary within 4000 tokens.

    Example Output:
    You have a total of 22 unread emails. Here's a summary:

    1.
       2023-09-14 5:07 PM
       drive-shares-dm-noreply@google.com  
       Spreadsheet shared with you: ""Alquiler quinta verano 2024""  
       Sin archivos adjuntos

    2.
       Date: 2023-09-14 5:05 PM
       Sender: nerdearla@nerdear.la
       Subject: 🚀 Mirá la agenda de Nerdearla 2023
       3 archivos adjuntos


    JSON data:
    {{$input}}

    BEGIN";

    public static string ConsolidatedSummaryFromWebsiteResults(string userPrompt, int tokenCount)
    {
        return @$"Generate a comprehensive, detailed summary based on the JSONDATA of WebsiteResults, integrating the data into one coherent piece of information that directly addresses the following user's query: ""{userPrompt}"".

        Your summary should:
        - Combine information from multiple website results present in the JSONDATA to form a singular, elaborated narrative or explanation.
        - Prioritize data that directly relates to the user's query.
        - Keep your summary within {tokenCount} tokens.
        - Structure the summary with subsections or bullet points for better clarity and understanding.
        - The summary must not contain any other elements than the summary itself.
        - THE SUMMARY MUST BE IN THE SAME LANGUAGE AS THE USER QUERY.

        [EXAMPLE]
        User Query: Find information about what is semantic kernel.
        Output:
        - Introduction: Semantic Kernel is a term often used in Natural Language Processing to refer to the underlying structure that allows for semantic understanding.
        - Technical Aspects: it involves using algorithms to parse text and understand context.
        - Applications: adds that Semantic Kernel technologies are increasingly used in AI and machine learning applications for tasks like sentiment analysis.

        JSONDATA:
        {{$input}}

        BEGIN";
    }
}
