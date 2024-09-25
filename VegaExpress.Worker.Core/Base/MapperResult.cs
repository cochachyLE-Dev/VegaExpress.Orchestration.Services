namespace Liiksoft.Data.Orm.Dapper
{
	public class Mapper<TResult>
	{
		public Mapper(TResult result, bool next = false) => (Result, Next) = (result, next);
		public TResult Result { get; set; }
		public bool Next { get; set; }
	}
}
