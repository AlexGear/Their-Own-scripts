using System;
using UnityEngine;

public class QueriesHitTriggers : IDisposable {
    private bool old;

    public QueriesHitTriggers(bool value) {
        old = Physics2D.queriesHitTriggers;
        Physics2D.queriesHitTriggers = value;
    }

    public void Dispose() {
        Physics2D.queriesHitTriggers = old;
    }
}
