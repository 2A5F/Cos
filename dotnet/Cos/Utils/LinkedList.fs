module Volight.Cos.Utils.LinkedListExt

open System.Collections.Generic

type 'T LinkedList with
    member inline s.Head = s.First
    member inline s.IsEmpty = s.Head = null
    member inline s.PushLast(v: 'T) = s.AddLast(v)
    member inline s.PushHead(v: 'T) = s.AddFirst(v)
    member inline s.PushLast(v: 'T LinkedListNode) = s.AddLast(v)
    member inline s.PushHead(v: 'T LinkedListNode) = s.AddFirst(v)
    member inline s.PopLast() = 
        let r = s.Last
        s.RemoveLast()
        r
    member inline s.PopHead() = 
        let r = s.Head
        s.RemoveFirst()
        r

type 'T LinkedListNode with
    member inline s.Prev = s.Previous
    member inline s.HasNext = s.Next <> null
    member inline s.HasPrev = s.Previous <> null
    member inline s.TryNext = if s.HasNext then Just s.Next else Nil
    member inline s.TryPrev = if s.HasPrev then Just s.Prev else Nil
    member inline s.GetNext = if s.HasNext then Just s.Next.Value else Nil
    member inline s.GetPrev = if s.HasPrev then Just s.Prev.Value else Nil
    member inline s.InsertNext(v: 'T) = s.List.AddAfter(s, v)
    member inline s.InsertPrev(v: 'T) = s.List.AddBefore(s, v)
    member inline s.InsertNext(v: 'T LinkedListNode) = s.List.AddAfter(s, v)
    member inline s.InsertPrev(v: 'T LinkedListNode) = s.List.AddBefore(s, v)
    member inline s.RemoveNext() = 
        let r = s.Next
        s.List.Remove(r)
        r
    member inline s.RemovePrev() = 
        let r = s.Prev
        s.List.Remove(r)
        r
    member inline s.Remove() = s.List.Remove(s)
