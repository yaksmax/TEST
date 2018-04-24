using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

++++

namespace lab2_enigma
{
	public class Rotor
	{
		const int capacity = 128;
		byte[] cipheredLetters;

		public int RotationsCount { get; private set; }

		//указатель на следующий ротор, который должен повернуться после полного проворота данного
		public Rotor Next { get; set; }

		public Rotor(int seed, byte rotPos=0)
		{
			RotationsCount = 0;
			var letters = new byte[capacity];
			for ( int i = 0; i < capacity; ++i )
				letters[i] = (byte)(i);

			//перемешиваем список символов в соответствии с заданной семечкой
			var rng = new Random( seed );
			cipheredLetters = letters.OrderBy( a => rng.Next() ).ToArray();

			for ( int i = 0; i < rotPos; ++i )
				Rotate();
		}

		public byte CipherByte(byte c)
		{
			return cipheredLetters[c];
		}
		public byte CipherByteBack(byte c)
		{
			for (int i = 0; i < capacity; ++i )
				if ( cipheredLetters[i] == c )
					return (byte)i;
			throw new Exception( "WAT" );
		}

		//поворачивает ротор, если провернулся целиком - поворачивает ещё и следующий
		public void Rotate()
		{
			++RotationsCount;

			byte head = cipheredLetters[0];
			for (int i = 0; i < capacity-1; ++i)
				cipheredLetters[i] = cipheredLetters[i+1];
			cipheredLetters[capacity-1] = head;

			if ( RotationsCount == capacity )
			{
				RotationsCount = 0;
				if ( Next != null )
					Next.Rotate();
			}
		}
	}

	public class Reflector
	{
		const int capacity = 128;
		const int capacity2 = 256;
		byte[] cipheredLetters = new byte[capacity];

		public Reflector(int seed)
		{
			var letters = new List<byte>( capacity2 );

			//элементы первой половины рефлектора будут ссылаться на вторую половину
			for ( int i = 0; i < capacity2; ++i )
				letters.Add( (byte)(capacity2+i) );

			//перемешиваем список символов
			var rng = new Random( seed );
			letters = letters.OrderBy( a => rng.Next() ).ToList();

			//заполняем первую половину перемешанными символами, а вторую: ссылками на первую
			for ( int i = 0; i < capacity2; ++i )
			{
				cipheredLetters[i] = letters[i];
				cipheredLetters[letters[i]] = (byte)i;
			}
		}

		public byte CipherByte(byte c)
		{
			return cipheredLetters[c];
		}
	}

	public class Enigma
	{
		Rotor Right, Middle, Left;
		public byte RotRight { get { return (byte)Right.RotationsCount; } }
		public byte RotMiddle { get { return (byte)Middle.RotationsCount; } }
		public byte RotLeft { get { return (byte)Left.RotationsCount; } }
		Reflector reflector;
		public Enigma(byte posLeft, byte posMiddle, byte posRight)
		{
			Left	= new Rotor( 1, posLeft );
			Middle	= new Rotor( 2, posMiddle );
			Right	= new Rotor( 3, posRight );
			Right.Next  = Middle;
			Middle.Next = Left;
			Left.Next   = Right;
			reflector = new Reflector( 4 );
		}

		public byte CipherByte(byte c)
		{
			c = Right.CipherByte( c );
			c = Middle.CipherByte( c );
			c = Left.CipherByte( c );
			c = reflector.CipherByte( c );
			c = Left.CipherByteBack( c );
			c = Middle.CipherByteBack( c );
			c = Right.CipherByteBack( c );
			Right.Rotate();
			return c;
		}

		public byte[] CipherString(byte[] str)
		{
			int strlen = str.Length;
			byte[] res = new byte[strlen];
			for ( int i = 0; i < strlen; ++i )
				res[i] = CipherByte( str[i] );
			return res;
		}
	}
}
