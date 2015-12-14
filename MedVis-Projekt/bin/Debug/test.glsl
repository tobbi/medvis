uniform sampler2D image;
uniform float fensterLow;
uniform float fensterHigh;

void main (void)  
{
   vec4 color = texture2D(image, gl_TexCoord[0].st); 
   //fensterLow = fensterLow/255.0;
   //fensterHigh = fensterHigh/255.0;

   if(color.r < fensterLow / 255.0) // r, g und b haben die gleichen Werte
   {
	 color.r = 0.0;
	 color.g = 0.0;
	 color.b = 0.0;
   }
   else if(color.r > fensterHigh / 255.0)
   {
	 color.r = 1.0;
	 color.g = 1.0;
	 color.b = 1.0;
   }
   else
   {
	 float n = color.r * 255.0;
	 float remappedVal = (fensterLow/255.0) + n * ((fensterHigh/255.0) - (fensterLow/255.0));
	 color.r = remappedVal;
	 color.g = remappedVal;
	 color.b = remappedVal;
   }
   gl_FragColor = color;
}