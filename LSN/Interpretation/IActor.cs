namespace LSN_Core
{
	/// <summary>
	/// Represents a character on the map.
	/// </summary>
	public interface IActor
	{
		int Health { get; }

		int MaxHP { get; }

		void ChangeHealth(int amount);

		int MP { get; }

		int MaxMP { get; }

		void ChangeMP(int amount, bool visible = false);

		int GetX();
		int GetY();
		void SetX(int value);
		void SetY(int value);
	}
}