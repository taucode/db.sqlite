(group
	:name "main"
	
	;====== CREATE ======
	(sequence
		(idle
			:name "root-node"
			:links-to ("end-of-clause"))

		("CREATE" :name "create")

		(alternatives
			:name "create-alternatives"

			(ref :path "../../create-table/")
			(ref :path "../../create-index/")
		)

		(end :name "end-of-clause")
	)

	;====== CREATE TABLE ======
	(sequence
		:name "create-table"

		("TABLE" :name "do-create-table")
		(identifier :name "table-name")
		("(" :name "table-opening")
		(ref
			:path "../column-def/"
			:name "column-definition-ref"
			:links-to ("table-closing")
		)
		("," :name "comma-after-column-def" :links-to ("column-definition-ref"))
		(ref
			:path "../constraint-defs/"
		)
		(")" :name "table-closing")
	)

	;====== COLUMN DEF ======
	(sequence
		:name "column-def"

		(identifier :name "column-name")
		(identifier :name "type-name")
		(optional :name "optional-precision"
			(sequence
				("(")
				(integer :name "precision")
				(optional :name "optional-scale"
					(sequence
						(",")
						(integer :name "scale")
					)
				)
				(")")
			)
		)
		(optional :name "nullability"
			(alternatives :name "null-or-not-null"
				("NULL" :name "null")
				(sequence
					("NOT" :name "not")
					("NULL" :name "not-null")
				)
			)
		)
		(optional :name "optional-inline-primary-key"
			(sequence
				("PRIMARY" :name "inline-primary")
				("KEY" :name "inline-primary-key")
			)
		)
		(optional
			("AUTOINCREMENT" :name "autoincrement")
		)
		(optional :is-exit t
			(sequence
				("DEFAULT" :name "default")
				(alternatives
					("NULL" :name "default-null")
					(integer :name "default-integer")
					(string :name "default-string")
				)
			)
		)
	)

	;====== CONSTRAINT DEFS ======
	(sequence
		:name "constraint-defs"

		("CONSTRAINT" :name "constraint")
		(identifier :name "constraint-name")
		(alternatives
			(ref :path "../../primary-key/")
			(ref :path "../../foreign-key/")
		)
		(splitter :is-exit t
			(idle :is-entrance t)

			("," :name "comma-after-constraint" :links-to ("../constraint"))
			(idle :is-exit t) ;;;;;; NB: _not_ joint! :is-exit is nil here.
		)
	)

	;====== PRIMARY KEY ======
	(sequence
		:name "primary-key"

		("PRIMARY" :name "do-primary")
		("KEY" :name "do-primary-key")
		(ref :path "../pk-columns/")
	)

	;====== PK COLUMNS ======
	(sequence
		:name "pk-columns"

		("(" :name "pk-opening")
		(identifier :name "pk-column-name")
		(optional
			(multi-word :values ("ASC" "DESC") :name "pk-asc-or-desc")
		)
		(splitter
			:name "more-pk-columns"

			(idle :is-entrance t)
			("," :name "comma-more-pk-columns" :links-to ("../pk-column-name"))
			(idle :is-exit t) ;;;;;; NB: _not_ joint! :is-exit is nil here.
		)
		(")" :name "pk-closing")
	)

	;====== FOREIGN KEY ======
	(sequence
		:name "foreign-key"

		("FOREIGN" :name "do-foreign")
		("KEY" :name "do-foreign-key")
		(ref :path "../fk-columns/")
		("REFERENCES" :name "fk-references")
		(identifier :name "fk-referenced-table-name")
		(ref :path "../fk-referenced-columns/")
	)

	;====== FK COLUMNS ======
	(sequence
		:name "fk-columns"

		("(" :name "fk-opening")
		(identifier :name "fk-column-name")
		(splitter
			(idle :is-entrance t)

			("," :name "comma-after-fk-column-name" :links-to ("../fk-column-name"))
			(idle :is-exit t) ;;;;;; NB: _not_ joint! :is-exit is nil here.
		)
		(")" :name "fk-closing")
	)

	;====== FK REFERENCED COLUMNS ======
	(sequence
		:name "fk-referenced-columns"

		("(" :name "fk-referenced-opening")
		(identifier :name "fk-referenced-column-name")
		(splitter
			(idle :is-entrance t)

			("," :name "comma-after-referenced-column-name" :links-to ("../fk-referenced-column-name"))
			(idle :is-exit t) ;;;;;; NB: _not_ joint! :is-exit is nil here.
		)
		(")" :name "fk-referenced-closing")
	)

	;====== CREATE INDEX ======
	(sequence
		:name "create-index"

		(alternatives
			(sequence
				("UNIQUE" :name "do-create-unique")
				("INDEX" :name "do-create-unique-index")
			)
			("INDEX" :name "do-create-non-unique-index")
		)

		(identifier :name "index-name")
		("ON" :name "index-on")
		(identifier :name "index-table-name")
		("(" :name "index-columns-opening")
		(identifier :name "index-column-name")
		(optional
			(multi-word :values ("ASC" "DESC") :name "index-column-asc-or-desc"))
		(splitter
			(idle :is-entrance t)

			("," :name "comma-after-index-column-name" :links-to ("../index-column-name"))
			(idle :is-exit t) ;;;;;; NB: _not_ joint! :is-exit is nil here.
		)
		(")" :name "index-columns-closing")
	)
)
