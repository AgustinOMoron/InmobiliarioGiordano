using UnityEngine;

public static class SupabaseConfig
{
    public const string URL = "https://snftysbccjomwxaleplb.supabase.co/rest/v1/";
    public const string API_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InNuZnR5c2JjY2pvbXd4YWxlcGxiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Nzc1MDQ3OTIsImV4cCI6MjA5MzA4MDc5Mn0.ekjGBXJvlVvJ5dudVHYkTsc0oMm6zcPps-UVJ48mIX0";
    public const string AUTH_URL = "https://snftysbccjomwxaleplb.supabase.co/auth/v1/signup";
    public const string SUPABASE_LOGIN_URL = "https://snftysbccjomwxaleplb.supabase.co/auth/v1/token?grant_type=password";
    public const int TIMEOUT_SECONDS = 10;
}
