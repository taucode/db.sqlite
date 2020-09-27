; CREATE
(defblock :name create :is-top t
	(exact-text :classes word :value "CREATE")
	(alt (block :ref create-table) (block :ref create-index))
	(end)
)

; CREATE TABLE
(defblock :name create-table
	(exact-text :classes word :value "TABLE" :name do-create-table)
	(some-text :classes identifier :name table-name)
	(exact-punctuation :value "(")
	(block :ref column-def :links table-closing next)
	(exact-punctuation :value "," :links column-def next)
	(block :ref constraint-defs)
	(exact-punctuation :value ")" :name table-closing)
)

; column definition
(defblock :name column-def
	(some-text :classes identifier :name column-name)
	(some-text :classes identifier :name type-name)
	(opt 
		(exact-punctuation :value "(")
		(some-integer :name precision)
		(opt
			(exact-punctuation :value ",")
			(some-integer :name scale)
		)
		(exact-punctuation :value ")")
	)
	(opt
		(alt
			(exact-text :classes word :value "NULL" :name null)
			(seq
				(exact-text :classes word :value "NOT")
				(exact-text :classes word :value "NULL" :name not-null)
			)
		)
	)
	(opt
		(exact-text :classes word :value "PRIMARY")
		(exact-text :classes word :value "KEY" :name inline-primary-key)
	)
	(opt
		(exact-text :classes word :value "AUTOINCREMENT" :name autoincrement)
	)
	(opt
		(exact-text :classes word :value "DEFAULT")
		(alt
			(exact-text :classes word :value "NULL" :name default-null)
			(some-integer :name default-integer)
			(some-text :classes string :name default-string)
		)
	)
)

; constraint definitions
(defblock :name constraint-defs
	(exact-text :classes word :value "CONSTRAINT" :name constraint)
	(some-text :classes identifier :name constraint-name)
	(alt (block :ref primary-key) (block :ref foreign-key))
	(alt
		(exact-punctuation :value "," :links constraint)
		(idle)
	)
)

; PRIMARY KEY
(defblock :name primary-key
	(exact-text :classes word :value "PRIMARY" :name do-primary-key)
	(exact-text :classes word :value "KEY")
	(block :ref pk-columns)
)

; PRIMARY KEY columns
(defblock :name pk-columns
	(exact-punctuation :value "(")
	(some-text :classes identifier :name pk-column-name)
	(opt (multi-text :classes word :values "ASC" "DESC" :name pk-asc-or-desc))
	(alt
		(exact-punctuation :value "," :links pk-column-name)
		(idle)
	)
	(exact-punctuation :value ")")
)

; FOREIGN KEY
(defblock :name foreign-key
	(exact-text :classes word :value "FOREIGN" :name do-foreign-key)
	(exact-text :classes word :value "KEY")
	(block :ref fk-columns)
	(exact-text :classes word :value "REFERENCES")
	(some-text :classes identifier :name fk-referenced-table-name)
	(block :ref fk-referenced-columns)
)

; FOREIGN KEY columns
(defblock :name fk-columns
	(exact-punctuation :value "(")
	(some-text :classes identifier :name fk-column-name)
	(alt
		(exact-punctuation :value "," :links fk-column-name)
		(idle)
	)
	(exact-punctuation :value ")")
)

; FOREIGN KEY referenced columns
(defblock :name fk-referenced-columns
	(exact-punctuation :value "(")
	(some-text :classes identifier :name fk-referenced-column-name)
	(alt
		(exact-punctuation :value "," :links fk-referenced-column-name)
		(idle)
	)
	(exact-punctuation :value ")")
)

; CREATE INDEX
(defblock :name create-index
	(opt (exact-text :classes word :value "UNIQUE" :name do-create-unique-index))
	(exact-text :classes word :value "INDEX" :name do-create-index)
	(some-text :classes identifier :name index-name)
	(exact-text :classes word :value "ON")
	(some-text :classes identifier :name index-table-name)
	(exact-punctuation :value "(")
	(some-text :classes identifier :name index-column-name)
	(opt (multi-text :classes word :values "ASC" "DESC" :name index-column-asc-or-desc))
	(alt
		(exact-punctuation :value "," :links index-column-name)
		(idle)
	)
	(exact-punctuation :value ")")
)
