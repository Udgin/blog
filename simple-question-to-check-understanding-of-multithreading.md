How to do planning of two threads so none of them can leave a circle (X is global value and default value is 0)?

```
while (X == 0)
{
    X = 1 - X;
}
```