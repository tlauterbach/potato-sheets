namespace JsonParser {

	internal class CharStream {

		private string m_input;
		private int m_index;


		public CharStream(string input) {
			Reset(input);
		}

		public void Reset(string input) {
			m_input = input;
			m_index = 0;
		}
		public void Reset() {
			m_index = 0;
		}

		public StringSlice Peek(int length = 1) {
			if (length <= 0 || IsEndOfStream(length)) {
				return StringSlice.Empty;
			} else {
				return new StringSlice(m_input, m_index, length);
			}
		}

		public void Advance(int length = 1) {
			if (length <= 0) {
				return;
			} else if (IsEndOfStream(length)) {
				m_index = m_input.Length;
			} else {
				m_index += length;
			}
		}

		public bool IsEndOfStream(int length = 1) {
			return m_index + (length - 1) >= m_input.Length;
		}

		

	}

}