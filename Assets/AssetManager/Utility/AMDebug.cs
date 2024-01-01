using UnityEngine;

namespace YH.AssetManage
{
	public class AMDebug
	{
		public static bool Frameable = true;

		public static string FormatFrame(string msg)
		{
			return string.Format("{0}#{1}", msg, Time.frameCount);
		}

		public static string FormatFrame(string format, params object[] args)
		{
			return FormatFrame(string.Format(format, args));
		}

		[System.Diagnostics.Conditional("AMLOG_ON")]
		public static void Log(object message)
		{
			if (Frameable)
			{
				Debug.Log(FormatFrame(message.ToString()));
			}
			else
			{
				Debug.Log(message);
			}
		}

		[System.Diagnostics.Conditional("AMLOG_ON")]
		public static void LogFormat(string format, params object[] args)
		{
			if (Frameable)
			{
				Debug.Log(FormatFrame(format,args));
			}
			else
			{
				Debug.LogFormat(format, args);
			}
		}

		[System.Diagnostics.Conditional("AMLOG_ON")]
		public static void LogWarning(object message)
		{
			if (Frameable)
			{
				Debug.LogWarning(FormatFrame(message.ToString()));
			}
			else
			{
				Debug.LogWarning(message);
			}
		}

		[System.Diagnostics.Conditional("AMLOG_ON")]
		public static void LogWarningFormat(string format, params object[] args)
		{
			if (Frameable)
			{
				Debug.LogWarning(FormatFrame(format, args));
			}
			else
			{
				Debug.LogWarningFormat(format, args);
			}
		}

		[System.Diagnostics.Conditional("AMLOG_ON"),System.Diagnostics.Conditional("AMLOG_ERROR_ON")]
		public static void LogError(object message)
		{
			if (Frameable)
			{
				Debug.LogError(FormatFrame(message.ToString()));
			}
			else
			{
				Debug.LogError(message);
			}
		}

		[System.Diagnostics.Conditional("AMLOG_ON"), System.Diagnostics.Conditional("AMLOG_ERROR_ON")]
		public static void LogErrorFormat(string format, params object[] args)
		{
			if (Frameable)
			{
				Debug.LogError(FormatFrame(format, args));
			}
			else
			{
				Debug.LogErrorFormat(format, args);
			}
		}

		[System.Diagnostics.Conditional("AMLOG_ON"), System.Diagnostics.Conditional("AMLOG_ERROR_ON")]
		public static void LogException(System.Exception exception, Object context)
		{
			Debug.LogException(exception, context);
		}

		[System.Diagnostics.Conditional("AMLOG_ON"), System.Diagnostics.Conditional("AMLOG_ERROR_ON")]
		public static void LogException(System.Exception exception)
		{
			Debug.LogException(exception);
		}

		[System.Diagnostics.Conditional("AMLOG_ON"), System.Diagnostics.Conditional("DEBUG")]
		public static void Assert(bool condition, string format, params object[] args)
		{
			if (!condition)
			{
				LogErrorFormat(format, args);
			}
		}
	}
}