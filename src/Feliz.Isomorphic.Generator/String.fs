namespace GobEx.Core

module String =
    let split (str:string) (delimiter:string): string array =
        str.Split delimiter