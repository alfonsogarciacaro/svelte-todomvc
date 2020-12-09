module Fable.Svelte

open System
open FSharp.Reflection
open Fable.Core
open Fable.Core.JsInterop

type Subscription<'Model> = 'Model -> unit
type Disposable = unit -> unit

type Store<'Model> =
    abstract subscribe: Subscription<'Model> -> Disposable

type WritableStore<'Model> =
    inherit Store<'Model>
    abstract update: ('Model -> 'Model) -> unit

type ElmishStore<'Model> =
    inherit Store<'Model>
    abstract on: msg: string * [<ParamArray>] fields: obj[] -> unit

let makeStoreWithMsgType (t: Type) (update: 'Msg -> 'Model -> 'Model) (init: 'Model) =
    let writable (_: 'Model): WritableStore<'Model> = importMember "svelte/store"
    let store = writable init
    let cases = FSharpType.GetUnionCases(t)
    { new ElmishStore<'Model> with
        member _.subscribe(s) = store.subscribe(s)
        member _.on(msg, fields) = store.update(fun model ->
            cases
            |> Array.tryFind (fun uci -> uci.Name = msg)
            |> Option.defaultWith (fun () -> failwith $"Unknown message {msg}")
            |> fun uci ->
                let msg = FSharpValue.MakeUnion(uci, fields) :?> _
                update msg model) }

let inline makeStore (update: 'Msg -> 'Model -> 'Model) (init: 'Model) =
    makeStoreWithMsgType typeof<'Msg> update init