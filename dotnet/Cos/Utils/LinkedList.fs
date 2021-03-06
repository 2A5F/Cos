module Volight.Cos.Utils.LinkedListExt

open System.Collections.Generic
open Volight.Cos.Utils.Utils

type 'T LinkedList with
    member inline s.Head = s.First
    member inline s.IsEmpty = isNull s.Head
    member inline s.IsOnlyOne = (not s.IsEmpty) && isSameObject s.Head s.Last
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
    member inline s.HasNext = notNull s.Next
    member inline s.HasPrev = notNull s.Previous
    member inline s.TryNext = if s.HasNext then Just s.Next else Nil
    member inline s.TryPrev = if s.HasPrev then Just s.Prev else Nil
    member inline s.GetNext = if s.HasNext then Just s.Next.Value else Nil
    member inline s.GetPrev = if s.HasPrev then Just s.Prev.Value else Nil
    member inline s.InsertNext(v: 'T, list: 'T LinkedList) = list.AddAfter(s, v)
    member inline s.InsertPrev(v: 'T, list: 'T LinkedList) = list.AddBefore(s, v)
    member inline s.InsertNext(v: 'T LinkedListNode, list: 'T LinkedList) = list.AddAfter(s, v)
    member inline s.InsertPrev(v: 'T LinkedListNode, list: 'T LinkedList) = list.AddBefore(s, v)
    member inline s.InsertNext(v: 'T) = s.List.AddAfter(s, v)
    member inline s.InsertPrev(v: 'T) = s.List.AddBefore(s, v)
    member inline s.InsertNext(v: 'T LinkedListNode) = s.List.AddAfter(s, v)
    member inline s.InsertPrev(v: 'T LinkedListNode) = s.List.AddBefore(s, v)
    member inline s.RemoveNext(list: 'T LinkedList) = 
        let r = s.Next
        list.Remove(r)
        r
    member inline s.RemovePrev(list: 'T LinkedList) = 
        let r = s.Prev
        list.Remove(r)
        r
    member inline s.Remove(list: 'T LinkedList) = list.Remove(s)
    member inline s.RemoveNext() = 
        let r = s.Next
        s.List.Remove(r)
        r
    member inline s.RemovePrev() = 
        let r = s.Prev
        s.List.Remove(r)
        r
    member inline s.Remove() = s.List.Remove(s)
    member inline s.Replace(v: 'T, list: 'T LinkedList) =
        let n = s.InsertNext(v, list)
        list.Remove(s)
        n
    member inline s.Replace(v: 'T LinkedListNode, list: 'T LinkedList) =
        let n = s.InsertNext(v, list)
        list.Remove(s)
        n
    member inline s.Replace(v: 'T) =
        let list = s.List
        let n = s.InsertNext(v, list)
        list.Remove(s)
        n
    member inline s.Replace(v: 'T LinkedListNode) =
        let list = s.List
        let n = s.InsertNext(v, list)
        list.Remove(s)
        n

