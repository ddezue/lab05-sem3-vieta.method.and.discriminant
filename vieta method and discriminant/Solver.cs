namespace vieta_method_and_discriminant
{
  public class Solver
  {
    public double X1;
    public double X2;
    public bool HasRoots;
    public string Details;
  }

  public static class QuadraticSolver
  {
    public static Solver Solve(double a, double b, double c, bool useVieta)
    {
      var res = new Solver();
      res.HasRoots = false;
      res.Details = "";

      if (Math.Abs(a) < 1e-10) {
        res.Details = "a не может быть 0";
        return res;
      }

      if (useVieta) {
        double S = -b / a;
        double P = c / a;
        double D = S * S - 4 * P;

        res.Details = $"S = {Math.Round(S, 3)}, P = {Math.Round(P, 3)}";

        if (D < 0) {
          return res;
        }

        res.X1 = (S + Math.Sqrt(D)) / 2;
        res.X2 = (S - Math.Sqrt(D)) / 2;
        res.HasRoots = true;
      }

      else {
        double D = b * b - 4 * a * c;
        res.Details = $"D = {Math.Round(D, 3)}";

        if (D < 0) {
        return res;
      }

        res.X1 = (-b + Math.Sqrt(D)) / (2 * a);
        res.X2 = (-b - Math.Sqrt(D)) / (2 * a);
        res.HasRoots = true;
      }

      return res;
    }
  }
}