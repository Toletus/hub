using Xunit;

// Registro/Notifier são estáticos — evita corrida entre classes.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
