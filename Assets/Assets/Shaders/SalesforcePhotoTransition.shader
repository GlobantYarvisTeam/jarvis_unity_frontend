Shader "Custom/SalesforcePhotoTransition" 
{
	Properties 
	{
		_Stencil ("Stencil Ref", Float) = 0
		tPhotoA ("Photo A", 2D) = "white" {}
		tPhotoB ("Photo B", 2D) = "white" {}
		tMixTexture ("Mix texture", 2D) = "white" {}
		mixRatio ("Mix ratio", Range (0.0, 1.0)) = 0
		threshold ("Threshold", Range (0.0, 1.0)) = 0
		useTexture ("Use Texture ", int ) = 1.0
	}

	SubShader 
	{
	    Tags { "Queue" = "Geometry" }
		Pass 
		{
			Cull Off
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			       
		    GLSLPROGRAM
		   
		    #ifdef VERTEX
		    varying vec2 vUv;

			void main() {
				gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
				vUv = gl_MultiTexCoord0.xy;
			}
	        #endif
		       
		    #ifdef FRAGMENT
		    uniform int useTexture;
			uniform float mixRatio, threshold;
			uniform sampler2D tPhotoA, tPhotoB, tMixTexture;
			varying vec2 vUv;

			void main() {
				float r, mixf;
				vec4 texel1, texel2, transitionTexel;

				texel1 = texture2D( tPhotoA, vUv );
				texel2 = texture2D( tPhotoB, vUv );

				if (useTexture == 1) {
					transitionTexel = texture2D( tMixTexture, vUv );
					r = mixRatio * ( 1.0 + threshold * 2.0 ) - threshold;
					mixf = clamp( ( transitionTexel.r - r ) * ( 1.0 / threshold ), 0.0, 1.0 );

					gl_FragColor = mix( texel1, texel2, mixf );
				} else {
					gl_FragColor = mix( texel2, texel1, mixRatio );
				}
			}
	        #endif
		       
	        ENDGLSL
	    }
	}
}