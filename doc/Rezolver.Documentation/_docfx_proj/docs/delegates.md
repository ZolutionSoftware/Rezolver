# Factory Delegates

You can register delegates in a Rezolver container so that, when the associated service type is resolved, your
delegate will be executed and its result returned as the instance.

The delegate can be of any type, subject to the following constraints:

- The delegate must have non-`void` return type
- It must not have any `ref` or `out` parameters


