using System;
using System.Collections.Generic;
using Priority_Queue;

class Vertex<T> : FastPriorityQueueNode
{
    public T data;
    public List<Vertex<T>> neighbors;
    private int weight;
    public int Weight
	{
		get
		{
			return weight;
		}
		set
		{
			this.weight = value;
		}
	}

	public Vertex(T data)
	{
		this.data = data;
		this.Weight = 1;
	}
}