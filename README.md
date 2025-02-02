# Hello Claude .NET

## Introduction

A really basic example of using the Anthropic API to access Claude using the plain REST API rather than using an SDK.

## Acknowledgments
Written in collaboration with Cursor and GitHub Copilot.

## Setup

Rename `.env-example` to `.env` and add your Anthropic API key.

Right now there are a few different functions you can run but you have to manually comment the ones you want to run in:

1. A straightforward question
2. Rewriting text from a PDF in modern English
3. Getting the named characters from text of a PDF

The latter two can be run against the PDF included in this project.

## Running

`dotnet run`

## Response:

You should get a response back that looks something like this:

```json
{
    "id":"msg_014yMMX9vaERV2ah3LrZaruU",
    "type":"message",
    "role":"assistant",
    "model":"claude-3-5-sonnet-20241022",
    "content":[
        {
            "type":"text",
            "text":"The capital of France is Paris. It is also the largest city in France and serves as the country's main political, economic, and cultural center."
        }
    ],
    "stop_reason":"end_turn",
    "stop_sequence":null,
    "usage":{
        "input_tokens":14,
        "cache_creation_input_tokens":0,
        "cache_read_input_tokens":0,
        "output_tokens":33
    }
}
```
Note that the responses may vary, for example another time I ran the same code I received this reply:
> "The capital of France is Paris."
