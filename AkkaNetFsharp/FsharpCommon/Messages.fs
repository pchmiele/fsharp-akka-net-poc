namespace FsharpCommon

type Message =
    | Value of int
    | Respond

type Work = 
    | SimpleTask
    | ComplicatedTask
