# List Injection

There are three primary types related to generic lists that are commonly used in 
.Net projects:

- @System.Collections.Generic.List`1
- @System.Collections.Generic.IList`1
- @System.Collections.Generic.IReadOnlyList`1

By default, Rezolver creates/injects an instance of one of these simply by binding to the
@System.Collections.Generic.List`1.%23ctor(System.Collections.Generic.IEnumerable{`0})
constructor - which means that [enumerable injection](../enumerables.md) must be enabled
unless you intend to take control of all of your `IEnumerable<T>` registrations.

# See Also

- [Enumerables](../enumerables.md)
- [Arrays](arrays.md)
- [Collections](collections.md)