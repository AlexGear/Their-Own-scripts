using System;
using UnityEngine;

public class QueriesStartInColliders : IDisposable {
    private bool old;

    public QueriesStartInColliders(bool value) {
        old = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = value;
    }

    public void Dispose() {
        Physics2D.queriesStartInColliders = old;
    }
}
