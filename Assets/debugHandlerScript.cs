using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class debugHandlerScript : MonoBehaviour
{
    GFG debugMessage_list;
    [SerializeField]
    GameObject debugPanel;
    [SerializeField]
    GameObject debugPanel_spawnTransform;
    GameObject canvas;
    public string message;
    private void Start()
    {
        GameEventsHandler.current.onDisplayDebugMessage += onDisplayDebug;
        debugMessage_list = new GFG();
        canvas = GameObject.Find("Canvas_FIRMsg");
    }


    public static void onDisplayDebug(string message)
    {

    }
}


class GFG
{
    //Head of list
    Node head;

    //Linked List Node
    public class Node
    {
        public string data;
        public Node next;
        public Node(string d)
        {
            data = d;
            next = null;
        }
    }

    //Given a key, deletes the first
    //occurence of key in linked list
    void deleteNode(string key)
    {
        // Store head node
        Node temp = head, prev = null;

        // If head node itself holds
        // the key to be deleted
        if (temp != null &&
            temp.data == key)
        {
            // Changed head
            head = temp.next;
            return;
        }

        // Search for the key to be
        // deleted, keep track of the
        // previous node as we need
        // to change temp.next
        while (temp != null &&
               temp.data != key)
        {
            prev = temp;
            temp = temp.next;
        }

        // If key was not present
        // in linked list
        if (temp == null)
            return;

        // Unlink the node from linked list
        prev.next = temp.next;
    }

    // Inserts a new Node at
    // front of the list.
    public void Push(string new_data)
    {
        Node new_node = new Node(new_data);
        new_node.next = head;
        head = new_node;
    }

    // This function prints contents
    // of linked list starting from
    // the given node
    public void printList()
    {
        Node tnode = head;
        while (tnode != null)
        {
            Debug.Log(tnode.data + " ");
            tnode = tnode.next;
        }
    }

    // This code is contributed by Rajput-Ji
    // Source: https://www.geeksforgeeks.org/linked-list-set-3-deleting-node/

    #region Main sample
        //GFG llist = new GFG();

        //llist.Push(7);
        //llist.Push(1);
        //llist.Push(3);
        //llist.Push(2);
 
        //Console.WriteLine("\nCreated Linked list is:");
        //llist.printList();
 
        //// Delete node with data 1
        //llist.deleteNode(1);
 
        //Console.WriteLine("\nLinked List after Deletion of 1:");
        //llist.printList();
    #endregion
}