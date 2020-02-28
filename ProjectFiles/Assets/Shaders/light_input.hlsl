#if SHADERGRAPH_PREVIEW
    Direction = half(0.5, 0.5, 0);
    Colour      = 1;
#else
    Light light = GetMainLight();
    Direction   = light.direction;
    Color       = light.color;
#endif
