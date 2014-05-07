using UnityEngine;
using System.Collections;
using System.IO;
using System.Threading;
using System;

	

public class PNGEncoder
{
	private  BitmapData img;
	private ByteArray byteout = new ByteArray();
	public bool isDone { private set; get; }
	private string path;
	
	public PNGEncoder( Texture2D texture, string path )
	{	
		isDone = false;
		
		this.path = path;
		// save out texture data to our own data structure
		img = new BitmapData(texture);
		
		Thread thread = new Thread(DoEncoding);
		thread.Start();		
		
//		if( blocking )
//			thread.Join();
		
		
	}
	
	public byte[] GetBytes ()
	{
		if(!isDone)
		{
			Debug.LogError("JPEGEncoder not complete, cannot get bytes!");
			return null;
		}
		
		return byteout.GetAllBytes();
	}
	
	
	/**
                 * Created a PNG image from the specified BitmapData
                 *
                 * @param image The BitmapData that will be converted into the PNG format.
                 * @return a ByteArray representing the PNG encoded image data.
                 * @langversion ActionScript 3.0
                 * @playerversion Flash 9.0
                 * @tiptext
                 */  
	
	/**
	* This is an AS3 class--just emulated the parts the encoder uses
	*/
	public class ByteArray
	{
		private MemoryStream stream;
		private BinaryWriter writer;
		
		public ByteArray()
		{
			stream = new MemoryStream();
			writer = new BinaryWriter(stream);
		}
		
		/**
		* Function from AS3--add a byte to our stream
		*/
		public void WriteByte( byte value  )
		{
			writer.Write(value);
		}
	
		/**
		* Function from AS3--add a byte to our stream
		*/
		public void WriteBytes( byte[] value  )
		{
			writer.Write(value);
		}
	
		public void WriteUnsignedInt( UInt32 value  )
		{
			writer.Write(value);
		}
	
		public void WriteInt( Int32 value  )
		{
			writer.Write(value);
		}
		
		/**
		* Spit back all bytes--to either pass via WWW or save to disk
		*/
		public byte[] GetAllBytes()
		{
			byte[] buffer = new byte[stream.Length];
			stream.Position = 0;
			stream.Read(buffer, 0, buffer.Length);
			
			return buffer;
		}
	
	    public byte ReadUnsignedByte()
		{
			return (byte)writer.BaseStream.ReadByte();	
		}
	
		public uint Length
		{
			get{ return (uint) writer.BaseStream.Length; }	
		}
	
		public uint Position
		{
			get{ return (uint) writer.BaseStream.Position; }	
			set{ writer.BaseStream.Position = value; }
		}
	}

/**
	* Another flash class--emulating the stuff the encoder uses
	*/
	public class BitmapData
	{
		public int height;
		public int width;
		public bool transparent = false;
	
		private Color32[] pixels;
		
		/**
		* Pull all of our pixels off the texture (Unity stuff isn't thread safe, and this is faster)
		*/
		public BitmapData ( Texture2D texture )
		{
			this.height = texture.height;
			this.width = texture.width;
			
			pixels = texture.GetPixels32();
		}
	
		/**
		* Mimic the flash function
		*/
		public Color32 GetPixelColor( int x, int y )
		{	
			x = Mathf.Clamp(x, 0, width - 1);
			y = Mathf.Clamp(y, 0, height - 1);
	
			return pixels[y * width + x];
		}
	
		/**	
		* Mimic the flash function
		*/
		public UInt32 GetPixel( int x, int y )
		{	
			x = Mathf.Clamp(x, 0, width - 1);
			y = Mathf.Clamp(y, 0, height - 1);
	
			Color32 color = pixels[y * width + x];
			UInt32 returnColor = (UInt32) ((color.r << 16) + (color.g << 8) + color.b); 
			return returnColor;
		}
	
		/**	
		* Mimic the flash function
		*/
		public UInt32 GetPixel32( int x, int y )
		{	
			x = Mathf.Clamp(x, 0, width - 1);
			y = Mathf.Clamp(y, 0, height - 1);
	
			Color32 color = pixels[y * width + x];
			UInt32 returnColor = (UInt32) ((color.a << 24) + (color.r << 16) + (color.g << 8) + color.b); 
			return returnColor;
		}
	

	}
	

	public struct BitString 
	{
		public int length;
		public int value;
	}
	
    private void DoEncoding() 
	{
        // Create output byte array
        byteout = new ByteArray();
        // Write PNG signature
        byteout.WriteUnsignedInt(0x89504e47);
        byteout.WriteUnsignedInt(0x0D0A1A0A);
        // Build IHDR chunk
        ByteArray IHDR = new ByteArray();
        IHDR.WriteInt(img.width);
        IHDR.WriteInt(img.height);
        IHDR.WriteUnsignedInt(0x08060000); // 32bit RGBA
        IHDR.WriteByte(0);
        writeChunk(byteout,0x49484452,IHDR);
        // Build IDAT chunk
        ByteArray IDAT = new ByteArray();
        for(int i = 0;i < img.height;i++) 
		{
            // no filter
            IDAT.WriteByte(0);
            uint p = 0;
            int j = 0;
            if ( !img.transparent )
			{
                for(j=0;j < img.width;j++)
				{
                    p = img.GetPixel(j,i);
                    IDAT.WriteUnsignedInt( (uint)(((p&0xFFFFFF) << 8)|0xFF) );
                }
            } 
			else 
			{
                for(j=0;j < img.width;j++) 
				{
                    p = img.GetPixel32(j,i);
                    IDAT.WriteUnsignedInt( (uint) (((p&0xFFFFFF) << 8)| (p>>24)));
                }
            }
        }
//        IDAT.compress();
        writeChunk(byteout,0x49444154,IDAT);
        // Build IEND chunk
        writeChunk(byteout,0x49454E44,null);
		
		isDone = true;
		
		if( !string.IsNullOrEmpty(path) )
			File.WriteAllBytes(path, GetBytes());
		
	}
    
    private static uint[] crcTable;
    private static bool crcTableComputed = false;

    private static void writeChunk(ByteArray png, uint type, ByteArray data)
	{
		uint c = 0;
        if (!crcTableComputed) 
		{
            crcTableComputed = true;
            crcTable = new uint[256 * 8];
            for (uint n = 0; n < 256; n++) 
			{
                c = n;
                for (int k = 0; k < 8; k++) 
				{
                    if((c & 1) == 1) 
					{
                        c = (uint)((uint)(0xedb88320) ^ (uint)(c >> 1));
                    } 
					else 
					{
                        c = (uint)(c >> 1);
                    }
                }
                crcTable[n] = c;
            }
        }
        uint len = 0;
        if (data != null) {
            len = data.Length;
        }
        png.WriteUnsignedInt(len);
        uint p = png.Position;
        png.WriteUnsignedInt(type);
        if ( data != null ) {
            png.WriteBytes(data.GetAllBytes());
        }
        uint e = png.Position;
        png.Position = p;
        c = 0xffffffff;
        for (int i = 0; i < (e-p); i++)
		{
            c = (uint)(crcTable[ (c ^ png.ReadUnsignedByte()) & (uint)(0xff)] ^ (uint)(c >> 8));
        }
        c = (uint)(c^(uint)(0xffffffff));
        png.Position = e;
        png.WriteUnsignedInt(c);
	}

}

/*
  Copyright (c) 2008, Adobe Systems Incorporated
  All rights reserved.

  Redistribution and use in source and binary forms, with or without 
  modification, are permitted provided that the following conditions are
  met:

  * Redistributions of source code must retain the above copyright notice, 
    this list of conditions and the following disclaimer.
  
  * Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the 
    documentation and/or other materials provided with the distribution.
  
  * Neither the name of Adobe Systems Incorporated nor the names of its 
    contributors may be used to endorse or promote products derived from 
    this software without specific prior written permission.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS
  IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
  PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/