Shader "SalesForce Polka Dot" {
    Properties {
        _noiseA ("Base (RGB)", 2D) = "white" {}
        _noiseB ("Base (RGB)", 2D) = "white" {}
        _mixture ("Mixture", Range (0.01, 1.0)) = 0
        _dotColor ("Color", Color) = (.34, .85, .92, 1)
        _alpha ("Alpha", Range (0.00, 1.0)) = 1.0
        _frequency ("Frequency", Range (0.01, 128.0) ) = 24
        _radius ("Radius", Range (0.01, 1.0) ) = 0.30
        _scaleX ("Scale X", Range (0.01, 1.0) ) = 1.0
        _scaleY ("Scale Y", Range (0.01, 1.0) ) = 1.0
    }
   
    SubShader {
        Tags { "Queue" = "Geometry" }
       
        Pass {
        	Cull Back
        	ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha 
        
            GLSLPROGRAM
           
            #ifdef VERTEX
            varying vec2 vUv;
           
            void main()
            {
                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
                vUv = gl_MultiTexCoord0.xy;
            }
            #endif
           
            #ifdef FRAGMENT
         
            uniform sampler2D _noiseA;
            uniform sampler2D _noiseB;
            uniform float _mixture;
            uniform float _alpha;
            uniform float _frequency;
            uniform float _radius;
            uniform float _scaleX;
            uniform float _scaleY;
            uniform vec4 _dotColor;
			varying vec2 vUv;
		
			void main()
			{
				float frequency = _frequency - fract(_frequency);
				float scaleX = _scaleX;
				float scaleY = _scaleY;
				//vec2 vUv2 = mat2(0.707, -0.707, 0.707, 0.707) * vUv;
			    vec2 nearest = 2.0 * fract(frequency * vec2(vUv.x * scaleX, vUv.y * scaleY)) - 1.0;
			    float dist = length(nearest);
			    float radius = _radius;
			    
			    
			    float noiseA = texture2D(_noiseA, vUv * vec2(scaleX, scaleY))[1] * (1.0 - _mixture);
				float noiseB = texture2D(_noiseB, vUv * vec2(scaleX, scaleY))[1] * _mixture;
				float noise = (noiseA + noiseB);

				vec3 uColor = _dotColor.rgb;

			    vec3 black = vec3(0.0, 0.0, 0.0);
			    vec3 fragcolor = mix(uColor, black, step(radius, dist));
				
				if(step(dist, radius) == 0.0)
				{
					discard;
				}
				else
				{
					gl_FragColor = vec4(fragcolor, (step(dist, radius) * noise * _alpha * (1.0 - vUv.y)) );
				}
			}
           
            #endif
           
            ENDGLSL
        }
    }
}
