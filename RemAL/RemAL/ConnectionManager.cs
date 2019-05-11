namespace RemAL {
	public interface ConnectionManager {
		bool IsActive();
		void Enable();
		void Disable();
		string GetName();
		void CreateTile(string path);
	}
}
