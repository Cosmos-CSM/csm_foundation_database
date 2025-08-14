﻿using CSM_Foundation.Database;

using CSM_Foundation_Database.Validations;

namespace CSM_Foundation_Database.Quality;
public record Q_EntityEvaluation<TEntity>
    where TEntity : IEntity {

    public (string, (BValidator, int)[])[] Expectations { get; init; } = [];
    public TEntity Mock { get; init; } = default!;

    public string Name { get; set; }

    public Q_EntityEvaluation(string Name) {
        this.Name = Name;
    }

    public Q_EntityEvaluation(string Name, TEntity Mock, (string, (BValidator, int)[])[] Expectations) {
        this.Name = Name;
        this.Mock = Mock;
        this.Expectations = Expectations;
    }
}
